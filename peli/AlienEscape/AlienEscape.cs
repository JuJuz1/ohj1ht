using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

namespace AlienEscape
{
    public class AlienEscape : PhysicsGame
    {
        private static readonly String[] kenttamj = {

            "XXXXXXXXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXXX   EX",
            "XXXXXXX   b     XXXX",
            "X   V    XXXXXXXXXXX",
            "X T     H      XXXXX",
            "XXXXXXXXXXXXXX     X",
            "XXXXX        XXXX  X",
            "X    BXXX  X X    XX",
            "X   XXX   XX     XXX",
            "X12 D   XXXXXXX XXXX",
            "===============A====",

        };

        private static readonly int tileWidth = 800 / (kenttamj[0].Length);
        private static readonly int tileHeight = 480 / kenttamj.Length;
        public override void Begin()
        {
            Level.Background.CreateGradient(Color.Blue, Color.White);
            TileMap kentta = TileMap.FromStringArray(kenttamj);

            kentta.SetTileMethod('X', LuoPalikka);
            kentta.SetTileMethod('=', LuoPalikka);
            // TODO: kentta.SetTileMethod('A', LuoPiikki);
            // TODO: kentta.SetTileMethod('V', LuoLaser);
            // TODO: kentta.SetTileMethod('T', LuoAarre);
            // TODO: kentta.SetTileMethod('D', LuoOvi);
            // TODO: kentta.SetTileMethod('B', LuoPainike);
            // TODO: kentta.SetTileMethod('b', LuoPainike2);
            // TODO: kentta.SetTileMethod('H', LuoHissi);
            // TODO: kentta.SetTileMethod('1', LuoPelaaja1);
            // TODO: kentta.SetTileMethod('2', LuoPelaaja2);
            // TODO: kentta.SetTileMethod('*', LuoVihollinen);
            // TODO: kentta.SetTileMethod('E', LuoExit);

            kentta.Execute(tileWidth, tileHeight); // Luodaan kenttä

            Level.CreateBorders();
            Camera.ZoomToLevel();

            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        }

        private void LuoPalikka(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject palikka = new PhysicsObject(leveys-3, korkeus-3);
            palikka.Position = paikka;
            //TODO: palikka.Image = 
            Add(palikka);
        }
    }
}