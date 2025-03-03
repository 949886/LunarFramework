#define PI 3.14159265359

#define RADIUS 0.25   // Radius of the circle
#define THICKNESS 0.05 // Thickness of the progress ring
#define ROTATION_SPEED 0.1  // Speed of rotation
#define PULSE_SPEED 0.06  // Speed of pulsing animation

float4 CircularLoadingIndicator(float2 uv, float iTime, float radius, float thickness, float rotation_speed)//, float2 iResolution)
{
    // Move origin to center of screen
    uv -= 0.5;

    // Set aspect ratio to ensure correct circle scaling
    // uv *= iResolution.xy / min(iResolution.x, iResolution.y);

    // Convert to polar coordinates
    float r = length(uv);
    float a = fmod((atan2(uv.y, uv.x) + PI) + iTime * rotation_speed, 2 * PI);

    // Circular progress (0 to 1)
    float progress = fmod(iTime * rotation_speed * PULSE_SPEED, 1.0);
    float angleProgress = progress * 2 * PI;

    float3 c = float3(1, 1, 1); 
    float alpha = 1; 

    // Create a circular ring
    float ring = step(radius - thickness, r) - step(radius, r);
    float ringa = smoothstep(radius - thickness, radius - thickness + 0.002, r) - 
                smoothstep(radius - 0.002, radius, r);

    // Fill the arc up to the current progress angle
    float arc = step(PI * (0.8 * (pow(sin(angleProgress), 1) + 1) + 0.25), a); // If angle is less than the progress, the pixel is drawn

    c *= arc * ring;
    alpha *= arc * ringa;

    // float3 color = lerp(float3(0.2, 0.5, 1.0), float3(1.0, 1.0, 1.0), progress);  // Gradually fade to white
    return float4(c, alpha);
}
