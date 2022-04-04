using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Palettes : MonoBehaviour
{
    public static Palettes instance;
    
    [Serializable]
    public struct Palette
    {
        public Color a, b, c, d;

        public Palette(Color a, Color b, Color c, Color d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    }
    
    public Palette[] paletteList = new Palette[1];
    public Palette colors;
    public PlayerCore playerCore;

    public SpriteRenderer[] bSprites = new SpriteRenderer[0];
    public SpriteRenderer[] cSprites = new SpriteRenderer[0];
    public SpriteRenderer[] dSprites = new SpriteRenderer[0];

    public Image[] bImages = new Image[0];
    public Image[] cImages = new Image[0];
    public Image[] dImages = new Image[0];

    public TMP_Text[] texts = new TMP_Text[2];

    
    public void Awake()
    {
        // Weird async loading bug... https://answers.unity.com/questions/1755002/start-and-awake-running-twice.html
        
        instance = this;
        colors = paletteList[UnityEngine.Random.Range(0, paletteList.Length)];
        playerCore.playerCam.cam.backgroundColor = colors.a;

        for (int i = 0; i < bSprites.Length; i++) bSprites[i].color = colors.b;
        for (int i = 0; i < cSprites.Length; i++) cSprites[i].color = colors.c;
        for (int i = 0; i < dSprites.Length; i++) dSprites[i].color = colors.d;
        
        for (int i = 0; i < bImages.Length; i++) bImages[i].color = colors.b;
        for (int i = 0; i < cImages.Length; i++) cImages[i].color = colors.c;
        for (int i = 0; i < dImages.Length; i++) dImages[i].color = colors.d;

        for (int i = 0; i < texts.Length; i++) texts[i].color = colors.d;
    }
}
