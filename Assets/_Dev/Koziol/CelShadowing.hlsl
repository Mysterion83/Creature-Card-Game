void CelShadowing_float(float Input, float Steps, out float Out)
{
    int stepCount = (int)Steps;
    float sum = 0.0;
    for (int i = 0; i <= stepCount; i++)
    {
        float edge = lerp(0.0, 1.0, (float)i / (float)stepCount);

        sum += step(edge, Input);
        sum += step(edge, -Input);
    }
    Out = sum / stepCount;
}