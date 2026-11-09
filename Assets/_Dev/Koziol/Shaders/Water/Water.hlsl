void WaterWaves_float(float3 Input, float Frequency, float Amplitude, float Time, out float3 Out)
{
    Out = Input;
    
    Out.y += sin(Input.x * Frequency + Time) * Amplitude;
    Out.y += cos(Input.z * Frequency + Time) * Amplitude;
    
    Out.y /= 2.0; // Average the two wave contributions
}