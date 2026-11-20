void CloudMovement_float(float3 InVector3, float Frequency, float Amplitude, float Time, out float3 OutVector3)
{
    OutVector3 = InVector3;
    
    float y = 0;
    float z = 0;
    float x = 0;
    
    y += sin(InVector3.x * Frequency + Time) * Amplitude;
    y += cos(InVector3.z * Frequency + Time) * Amplitude;
    
    x += -sin(InVector3.x * Frequency + Time) * Amplitude;
    x += cos(InVector3.z * Frequency + Time) * Amplitude;
    
    z += -sin(InVector3.x * Frequency + Time) * Amplitude;
    z += -cos(InVector3.z * Frequency + Time) * Amplitude;
    
    y /= 2.0;
    x /= 2.0;
    z /= 2.0;
    
    OutVector3.y += y;
    OutVector3.x += x;
    OutVector3.z += z;
}