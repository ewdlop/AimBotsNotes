using ClickableTransparentOverlay;
using ImGuiNET;

public class Renderer : Overlay
{
    public bool aimbot;
    public bool aimBotOnTeam;
    public bool wallHack;
    protected override void Render()
    {
        ImGui.BeginMenu("menu");
        ImGui.Checkbox("aimbot", ref aimbot);
        ImGui.Checkbox("aimbot on team", ref aimBotOnTeam);
        ImGui.Checkbox("wallhack", ref wallHack);
    }
}
