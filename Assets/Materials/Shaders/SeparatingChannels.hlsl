    void SeparateChannels_float(float Input_R, float Input_G, float Input_B, float IN_Theshold, float OUT_Theshold,
                      out float R_Map, out float G_Map, out float B_Map, out float Y_Map, out float C_Map, out float M_Map, out float W_Map, out float Bl_Map)
{
    float condRl = Input_R < OUT_Theshold;
    float condGl = Input_G < OUT_Theshold;
    float condBl = Input_B < OUT_Theshold;
    float condRm = Input_R > IN_Theshold;
    float condGm = Input_G > IN_Theshold;
    float condBm = Input_B > IN_Theshold;
                
    R_Map = condRm * condGl * condBl;
    G_Map = condRl * condGm * condBl;
    B_Map = condRl * condGl * condBm;
    Y_Map = condRm * condGm * condBl;
    C_Map = condRl * condGm * condBm;
    M_Map = condRm * condGl * condBm;
    W_Map = condRm * condGm * condBm;
    Bl_Map = condRl * condGl * condBl;
}