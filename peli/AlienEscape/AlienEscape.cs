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
        private static readonly String[] kentta = {

        };

        private static readonly int tileWidth = 800 / kentta[0].Length;
        private static readonly int tileHeight = 480 / kentta.Length;
        public override void Begin()
        {
            Level.Background.CreateGradient(Color.Blue, Color.White);
            TileMap kentta = TileMap.FromStringArray(kentta);

            kentta.SetTileMethod('X', LuoSeina);
            kentta.SetTileMethod('=', LuoLattia);
            kentta.SetTileMethod('A', LuoPiikki);
            kentta.SetTileMethod('V', LuoLaser);
            kentta.SetTileMethod('T', LuoAarre);
            kentta.SetTileMethod('D', LuoOvi);
            kentta.SetTileMethod('B', LuoPainike);
            kentta.SetTileMethod('b', LuoPainike2);
            kentta.SetTileMethod('H', LuoHissi);
            kentta.SetTileMethod('P', LuoPelaaja1);
            kentta.SetTileMethod('p', LuoPelaaja2);
            kentta.SetTileMethod('*', LuoVihollinen);
            kentta.SetTileMethod('E', LuoExit);

            kentta.Execute(tileWidth, tileHeight); // Luodaan kenttä

            Level.CreateBorders();
            Camera.ZoomToLevel();

            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        }
    }
}