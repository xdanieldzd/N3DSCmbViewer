#version 120
#extension GL_EXT_gpu_shader4 : enable
#extension GL_EXT_geometry_shader4 : enable

#define MAX_BONE_MATRIX 128

/* General */
varying vec4 vertexPosition;

/* For lighting calculations */
varying vec4 diffuse, ambient;
varying vec3 normal, halfVector;

/* Data from cmb */
uniform usamplerBuffer vertBoneSampler;

uniform mat4 boneMatrix[MAX_BONE_MATRIX];
uniform bool perVertexSkinning;
uniform int boneId;
uniform float vertexScale;
uniform float texCoordScale;
uniform float normalScale;

/* Settings */
uniform bool enableLighting;
uniform bool enableSkeletalStuff;

void main()
{
    /* Lighting */
    if(enableLighting)
    {
        normal = normalize((gl_NormalMatrix * gl_Normal) * normalScale);
        halfVector = gl_LightSource[0].halfVector.xyz;
        
        diffuse = gl_FrontMaterial.diffuse * gl_LightSource[0].diffuse;
        ambient = gl_FrontMaterial.ambient * gl_LightSource[0].ambient;
        ambient += gl_LightModel.ambient * gl_FrontMaterial.ambient;
    }

    /* Bone workings */
    mat4 tempMatrix = mat4(1.0);
    
    if(enableSkeletalStuff)
    {
        int lookupId = int(texelFetchBuffer(vertBoneSampler, gl_VertexID).r);
        if(perVertexSkinning)
        {
            /* Per vertex OR per vertex, no translation*/
            tempMatrix = boneMatrix[lookupId];
        }
        else
        {
            /* Single bone */
            tempMatrix = boneMatrix[boneId];
        }
    }
    
    /* Vertex positioning */
    vec4 vertex = vec4(gl_ModelViewMatrix * (tempMatrix * (gl_Vertex * vertexScale)));
    vertexPosition = gl_ProjectionMatrix * vertex;
    
    /* Finalize */
    gl_Position = vertexPosition;
    gl_TexCoord[0] = gl_MultiTexCoord0 * texCoordScale;

    /* TEST: gl_VertexID support */
    //float test = float(gl_VertexID) / 255.0;
    //gl_Color = vec4(test, 0.0, 0.0, 1.0);
    
    gl_FrontColor = gl_Color;
}
