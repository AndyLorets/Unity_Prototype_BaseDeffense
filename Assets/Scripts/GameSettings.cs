using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Pressets/GameSettings")]
public class GameSettings : ScriptableObject
{
    public enum CrosshairMode { World3D, ScreenUI }

    public CrosshairMode crosshairMode = CrosshairMode.World3D;
}
