void MultAdd_float(float3 Albedo1, float3 Normal1, float Smoothness1, float AO1,
                   float3 Albedo2, float3 Normal2, float Smoothness2, float AO2,
                   float3 Albedo3, float3 Normal3, float Smoothness3, float AO3,
                   float3 Albedo4, float3 Normal4, float Smoothness4, float AO4,
                   float3 Albedo5, float3 Normal5, float Smoothness5, float AO5,
                   float3 Albedo6, float3 Normal6, float Smoothness6, float AO6,
                   float3 Albedo7, float3 Normal7, float Smoothness7, float AO7,
                   float3 Albedo8, float3 Normal8, float Smoothness8, float AO8,
                   out float3 Albedo, out float3 Normal, out float Smoothness, out float AO)
{
    Albedo = Albedo1 + Albedo2 + Albedo3 + Albedo4 + Albedo5 + Albedo6 + Albedo7 + Albedo8;
    Normal = Normal1 + Normal2 + Normal3 + Normal4 + Normal5 + Normal6 + Normal7 + Normal8;
    Smoothness = Smoothness1 + Smoothness2 + Smoothness3 + Smoothness4 + Smoothness5 + Smoothness6 + Smoothness7 + Smoothness8;
    AO = AO1 + AO2 + AO3 + AO4 + AO5 + AO6 + AO7 + AO8;
}