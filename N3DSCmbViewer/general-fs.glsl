#version 120

/* General */
varying vec4 vertexPosition;

/* For lighting calculations */
varying vec4 diffuse, ambient;
varying vec3 normal, halfVector;

/* Texture sampler */
uniform sampler2D tex0;
uniform sampler2D tex1;

/* Data from cmb */
uniform vec4 materialColor;

/* Settings */
uniform bool disableAlpha;
uniform bool enableLighting;

void main()
{
    vec4 color = vec4(1.0, 1.0, 1.0, 1.0);
    vec3 n = normalize(normal);
    vec3 lightDir = vec3(gl_LightSource[0].position.xyz);
    
    /* Lighting */
    if(enableLighting)
    {
        vec3 halfV;
        float NdotL, NdotHV;

        color = ambient;
        NdotL = max(dot(n, lightDir), 0.0);
        if (NdotL > 0.0)
        {
            color += diffuse * NdotL;
            halfV = normalize(halfVector);
            NdotHV = max(dot(n, halfV), 0.0);
            color += gl_FrontMaterial.specular * gl_LightSource[0].specular * pow(NdotHV, gl_FrontMaterial.shininess);
        }
    }

    /* Texturing */
    vec4 textureColor0 = texture2D(tex0, gl_TexCoord[0].st * vec2(1.0, -1.0));
    vec4 textureColor1 = texture2D(tex1, gl_TexCoord[0].st * vec2(1.0, -1.0));
    vec4 textureMixed = textureColor0;   // * textureColor1;

    /* Finalize */
    gl_FragColor = textureMixed * color * (gl_Color * materialColor);

    if(disableAlpha)
    {
        gl_FragColor.a = 1.0;
    }

    /* HACK: brightness */
    //gl_FragColor *= vec4(2.0, 2.0, 2.0, 1.0);
}
