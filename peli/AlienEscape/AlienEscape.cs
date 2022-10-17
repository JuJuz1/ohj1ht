using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace AlienEscape
{
    public class AlienEscape : PhysicsGame
    {
        // TODO: Kentän lataaminen tekstitiedostosta
        private static readonly String[] kentta1 = {
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
            "XXXXXXXXXXXXXXXAXXXX",
            };

        /// <summary>
        /// Määritellään pelikentän yhden ruudun leveys ja korkeus
        /// </summary>
        private static readonly int tileWidth = 1600 / kentta1[0].Length;
        private static readonly int tileHeight = 960 / kentta1.Length;

        /// <summary>
        /// Ladataan pelissä tarvittavat kuvat
        /// </summary>
        private static readonly Image luolanKuva = LoadImage("luola.png");
        private static readonly Image seinanKuva = LoadImage("seina1");
        private static readonly Image piikinKuva = LoadImage("piikki.png");

        /// <summary>
        /// Peli aloitetaan
        /// </summary>
        public override void Begin()
        {
            LuoKentta();

            Gravity = new Vector(0, -500);

            Level.CreateBorders();
            Camera.ZoomToLevel();

            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

            Image piikki1 = LoadImage("piikki.png");
            Shape piikki2 = Shape.FromImage(piikki1);

        }


        /// <summary>
        /// Luodaan kenttä merkkijonotaulukosta
        /// </summary>
        private void LuoKentta()
        {
            TileMap kentta = TileMap.FromStringArray(kentta1);
            Level.Background.Image = luolanKuva;
            kentta.SetTileMethod('X', LuoPalikka);
            kentta.SetTileMethod('A', LuoPiikki);
            // TODO: kentta.SetTileMethod('V', LuoLaser);
            // TODO: kentta.SetTileMethod('T', LuoAarre);
            // TODO: kentta.SetTileMethod('D', LuoOvi);
            // TODO: kentta.SetTileMethod('B', LuoPainike1);
            // TODO: kentta.SetTileMethod('b', LuoPainike2);
            // TODO: kentta.SetTileMethod('H', LuoHissi);
            // TODO: kentta.SetTileMethod('1', LuoPelaaja1);
            // TODO: kentta.SetTileMethod('2', LuoPelaaja2);
            // TODO: kentta.SetTileMethod('*', LuoVihollinen);
            // TODO: kentta.SetTileMethod('E', LuoExit);

            kentta.Execute(tileWidth, tileHeight); // Luodaan kenttä
        }


        /// <summary>
        /// Luodaan kenttään palikka
        /// </summary>
        /// <param name="paikka">Piste, johon palikka syntyy</param>
        /// <param name="leveys">Palikan leveys</param>
        /// <param name="korkeus">Palikan korkeus</param>
        private void LuoPalikka(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject palikka = PhysicsObject.CreateStaticObject(leveys-3, korkeus-3);
            palikka.Position = paikka;
            palikka.Image = seinanKuva;
            // if (RandomGen.NextBool()) Image.Flip(kuva);
            // if (RandomGen.NextBool()) Image.Mirror(kuva);
            // palikka.Image = kuva;
            Add(palikka);
        }


        /// <summary>
        /// Luodaan piikki
        /// </summary>
        /// <param name="paikka">Piste, johon piikki luodaan</param>
        /// <param name="leveys">Piikin leveys</param>
        /// <param name="korkeus">Piikin korkeus</param>
        private void LuoPiikki(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject piikki = PhysicsObject.CreateStaticObject(leveys*1.5, korkeus);
            piikki.Image = piikinKuva;
            piikki.Position = paikka;
            Add(piikki);
        }
    }
}