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

        private static PlatformCharacter pelaaja1;
        private static PlatformCharacter pelaaja2;

        /// <summary>
        /// Määritellään pelikentän yhden ruudun leveys ja korkeus
        /// </summary>
        private static readonly int tileWidth = 1600 / kentta1[0].Length;
        private static readonly int tileHeight = 960 / kentta1.Length;

        /// <summary>
        /// Luodaan muuttujat pelaajien aloituspisteille
        /// </summary>
        private static Vector pelaaja1Aloitus;
        private static Vector pelaaja2Aloitus;

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
            LuoPelaaja(pelaaja1Aloitus, tileWidth, tileHeight, "pelaaja1", Color.Blue);
            LuoPelaaja(pelaaja2Aloitus, tileWidth, tileHeight, "pelaaja2", Color.Red);

            Gravity = new Vector(0, -500);

            Level.CreateBorders();
            Camera.ZoomToLevel();

            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

            // TODO: pelaajien liikkuminen
            // Keyboard.Listen(Key.A, ButtonState.Down, LiikutaPelaajaa, "pelaaja1", -10.0, "Pelaaja 1: Kävele vasemmalle");

            // TODO: Poista 2 seuraavaa riviä? Tai sitten siirretään muualle?
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
            kentta.SetTileMethod('1', AsetaPelaajanPaikka, "pelaaja1");
            kentta.SetTileMethod('2', AsetaPelaajanPaikka, "pelaaja2");
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


        /// <summary>
        /// Asetetaan muuttujaan pelaaja1Aloitus tai pelaaja2Aloitus kyseisen pelaajan aloituspiste
        /// </summary>
        /// <param name="paikka">Piste, joka tallennetaan muuttujaan</param>
        /// <param name="pelaaja">Pelaaja, jonka muuttujaan paikka tallennetaan ("pelaaja1" tai "pelaaja2")</param>
        /// <param name="korkeus">ei käytetä</param>
        /// <param name="leveys">ei käytetä</param>
        private void AsetaPelaajanPaikka(Vector paikka, double leveys, double korkeus, string pelaaja)
        {
            if (pelaaja == "pelaaja1") pelaaja1Aloitus = paikka;
            if (pelaaja == "pelaaja2") pelaaja2Aloitus = paikka;
        }


        /// <summary>
        /// Luodaan pelaaja
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">Pelaajan leveys</param>
        /// <param name="korkeus">Pelaajan korkeus</param>
        /// <param name="tunniste">Merkkijono, jolla eri pelaajat erotetaan toisistaan</param>
        /// <param name="vari">Pelaajan väri</param>
        private void LuoPelaaja(Vector paikka, double leveys, double korkeus, string tunniste, Color vari)
        {
            PlatformCharacter pelaaja = new PlatformCharacter(leveys / 2, korkeus, Shape.Rectangle);
            pelaaja.Position = paikka;
            // TODO: Pelaajien kuvat
            pelaaja.Color = vari;
            pelaaja.Tag = tunniste;
            Add(pelaaja);
        }


        // TODO: LiikutaPelaajaa
        private void LiikutaPelaajaa(string tunniste, double kavelyNopeus)
        {

        }
    }
}