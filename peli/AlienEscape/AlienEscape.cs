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
        public override void Begin()
        {
            LuoKentta();

            Gravity = new Vector(0, -500);

            Level.CreateBorders();
            Camera.ZoomToLevel();

            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        }

        /// <summary>
        /// Luodaan kenttä
        /// </summary>
        private void LuoKentta()
        {
            String[] kenttamj = {

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

            int tileWidth = 1600 / (kenttamj[0].Length);
            int tileHeight = 960 / kenttamj.Length;

            Level.Background.CreateGradient(Color.BloodRed, Color.Azure);
            TileMap kentta = TileMap.FromStringArray(kenttamj);

            kentta.SetTileMethod('X', LuoPalikka);
            kentta.SetTileMethod('=', LuoPalikka);
            kentta.SetTileMethod('A', LuoPiikki);
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
        }
    /// <summary>
    /// Luodaan kentän rakenneosat
    /// </summary>
    /// <param name="paikka">Mihin palikka syntyy</param>
    /// <param name="leveys">Palikan leveys</param>
    /// <param name="korkeus">Palikan korkeus</param>
    private void LuoPalikka(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject palikka = PhysicsObject.CreateStaticObject(leveys-3, korkeus-3);
            palikka.Position = paikka;
            //TODO: palikka.Image = 
            Add(palikka);
        }

        /// <summary>
        /// Luodaan piikki
        /// </summary>
        /// <param name="paikka"></param>
        /// <param name="leveys"></param>
        /// <param name="korkeus"></param>
        private void LuoPiikki(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject piikki = PhysicsObject.CreateStaticObject(leveys - 3, korkeus - 3);
            piikki.Position = paikka;
            //TODO: piikki.Image = 
            Add(piikki);
        }
    }
}