using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private static PhysicsObject ovi;
        private static GameObject ovenPainike;
        private static IntMeter pelaaja1HP;
        private static IntMeter pelaaja2HP;

        /// <summary>
        /// Määritellään pelikentän yhden ruudun leveys ja korkeus
        /// </summary>
        private static readonly int tileWidth = 1600 / kentta1[0].Length;
        private static readonly int tileHeight = 960 / kentta1.Length;

        /// <summary>
        /// Luodaan muuttujat objektien aloituspisteille
        /// </summary>
        private static Vector pelaaja1Aloitus;
        private static Vector pelaaja2Aloitus;
        private static Vector ovenPaikka;
        private static Vector ovenPainikkeenPaikka;

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
            pelaaja1 = LuoPelaaja(pelaaja1Aloitus, tileWidth, tileHeight, "pelaaja1", Color.Blue);
            pelaaja2 = LuoPelaaja(pelaaja2Aloitus, tileWidth, tileHeight, "pelaaja2", Color.Red);
            ovi = LuoOvi(ovenPaikka, tileWidth, tileHeight);
            ovenPainike = LuoOvenPainike(ovenPainikkeenPaikka, tileWidth, tileHeight);

            Gravity = new Vector(0, -1000);

            Level.CreateBorders();
            Camera.ZoomToLevel();

            LuoHPLaskuri1();
            LuoHPLaskuri2();
            AddCollisionHandler(pelaaja1, "piikki", Pelaaja1Vahingoittui);
            AddCollisionHandler(pelaaja2, "piikki", Pelaaja2Vahingoittui);

            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");

            // TODO: Ohjaimien ryhmittely?
            Keyboard.Listen(Key.A, ButtonState.Down, pelaaja1.Walk, "Pelaaja 1: Kävele vasemmalle", -180.0);
            Keyboard.Listen(Key.D, ButtonState.Down, pelaaja1.Walk, "Pelaaja 1: Kävele oikealle", 180.0);
            Keyboard.Listen(Key.W, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 1: Hyppää", pelaaja1, 600.0);
            Keyboard.Listen(Key.S, ButtonState.Pressed, KaytaObjektia, "Pelaaja 1: Paina nappia / poimi esine", pelaaja1);

            Keyboard.Listen(Key.Left, ButtonState.Down, pelaaja2.Walk, "Pelaaja 2: Kävele vasemmalle", -180.0);
            Keyboard.Listen(Key.Right, ButtonState.Down, pelaaja2.Walk, "Pelaaja 2: Kävele oikealle", 180.0);
            Keyboard.Listen(Key.Up, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 2: Hyppää", pelaaja2, 600.0);
            Keyboard.Listen(Key.Down, ButtonState.Pressed, KaytaObjektia, "Pelaaja 2: Paina nappia / poimi esine", pelaaja2);

            // TODO: Tarvitaanko 2 seuraavaa riviä mihinkään, voiko ne poistaa?
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
            kentta.SetTileMethod('D', AsetaPaikka, "ovi");
            kentta.SetTileMethod('B', AsetaPaikka, "ovenPainike");
            // TODO: kentta.SetTileMethod('b', LuoPainike2);
            // TODO: kentta.SetTileMethod('H', LuoHissi);
            kentta.SetTileMethod('1', AsetaPaikka, "pelaaja1");
            kentta.SetTileMethod('2', AsetaPaikka, "pelaaja2");
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
            PhysicsObject palikka = PhysicsObject.CreateStaticObject(leveys, korkeus);
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
            piikki.Tag = "piikki";
            Add(piikki);
        }


        /// <summary>
        /// Luodaan peliin ovi
        /// </summary>
        /// <param name="paikka">Oven paikka</param>
        /// <param name="leveys">Oven leveys</param>
        /// <param name="korkeus">Oven korkeus</param>
        private PhysicsObject LuoOvi(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject ovi = PhysicsObject.CreateStaticObject(leveys * 0.6, korkeus);
            // TODO: ovi.Image = ovenKuva;
            ovi.Position = paikka;
            ovi.Shape = Shape.Rectangle;
            ovi.Color = Color.Brown;
            Add(ovi);
            return ovi;
        }


        /// <summary>
        /// Luodaan peliin painike
        /// </summary>
        /// <param name="paikka">Painikkeen paikka</param>
        /// <param name="leveys">Painikkeen leveys</param>
        /// <param name="korkeus">Painikkeen korkeus</param>
        private GameObject LuoOvenPainike(Vector paikka, double leveys, double korkeus)
        {
            GameObject painike = new GameObject(leveys * 0.2, korkeus * 0.2);
            // TODO: painike.Image = painikkeenKuva;
            painike.Position = paikka;
            painike.Shape = Shape.Rectangle;
            painike.Color = Color.Red;
            Add(painike);
            return painike;
        }


        /// <summary>
        /// Asetetaan muuttujaan objektin paikka
        /// </summary>
        /// <param name="paikka">Piste, joka tallennetaan muuttujaan</param>
        /// <param name="pelaaja">Objekti, jonka muuttujaan paikka tallennetaan ("pelaaja1", "pelaaja2", "ovi" tai "ovenPainike")</param>
        /// <param name="korkeus">ei käytetä</param>
        /// <param name="leveys">ei käytetä</param>
        private void AsetaPaikka(Vector paikka, double leveys, double korkeus, string pelaaja)
        {
            if (pelaaja == "pelaaja1") pelaaja1Aloitus = paikka;
            if (pelaaja == "pelaaja2") pelaaja2Aloitus = paikka;
            if (pelaaja == "ovi") ovenPaikka = paikka;
            if (pelaaja == "ovenPainike") ovenPainikkeenPaikka = paikka;
        }


        /// <summary>
        /// Luodaan pelaaja
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">Pelaajan leveys</param>
        /// <param name="korkeus">Pelaajan korkeus</param>
        /// <param name="tunniste">Merkkijono, jolla eri pelaajat erotetaan toisistaan</param>
        /// <param name="vari">Pelaajan väri</param>
        private PlatformCharacter LuoPelaaja(Vector paikka, double leveys, double korkeus, string tunniste, Color vari)
        {
            PlatformCharacter pelaaja = new PlatformCharacter(leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
            pelaaja.Position = paikka;
            // TODO: Pelaajien kuvat
            pelaaja.Color = vari;
            pelaaja.Tag = tunniste;
            Add(pelaaja);
            return pelaaja;
        }


        /// <summary>
        /// Luodaan pelaajan 1 hitpoint laskuri ja sen näyttö
        /// </summary>
        private void LuoHPLaskuri1()
        {
            pelaaja1HP = new IntMeter(3);
            pelaaja1HP.MinValue = 0;
            Label nayttoHP1 = new Label();
            nayttoHP1.X = Screen.Left + 150;
            nayttoHP1.Y = Screen.Top - 50;
            nayttoHP1.TextColor = Color.White;
            nayttoHP1.Color = Color.Blue;
            nayttoHP1.IntFormatString = " HP = {0:D1} ";
            nayttoHP1.BindTo(pelaaja1HP);
            Add(nayttoHP1);
        }


        /// <summary>
        /// Luodaan pelaajan 2 hitpoint laskuri ja sen näyttö
        /// </summary>
        private void LuoHPLaskuri2()
        {
            pelaaja2HP = new IntMeter(3);
            pelaaja2HP.MinValue = 0;
            Label nayttoHP2 = new Label();
            nayttoHP2.X = Screen.Left + 300;
            nayttoHP2.Y = Screen.Top - 50;
            nayttoHP2.TextColor = Color.White;
            nayttoHP2.Color = Color.Red;
            nayttoHP2.IntFormatString = " HP = {0:D1} ";
            nayttoHP2.BindTo(pelaaja2HP);
            Add(nayttoHP2);
        }


        /// <summary>
        /// Pelaaja hyppää
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka hyppää</param>
        /// <param name="hyppaysnopeus">Nopeus, jolla pelaaja hyppää</param>
        private void PelaajaHyppaa(PlatformCharacter pelaaja, double hyppaysnopeus)
        {
            pelaaja.Jump(hyppaysnopeus); // Täytyi tehdä oma aliohjelma, koska ohjainten luonti valitti siitä, että PlatformCharacter.Jump() palauttaa arvon,
                                         // jolloin sitä ei voinut suoraan kutsua suoraan W-näppäintä painettaessa
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pelaaja"></param>
        private void KaytaObjektia(PhysicsObject pelaaja)
        {
            if (Math.Abs(pelaaja.X - ovenPainike.X) < tileWidth * 0.3 && Math.Abs(pelaaja.X - ovenPainike.X) < tileHeight * 0.3) // Avataan ovi
            {
                ovenPainike.Color = Color.Green;
                // TODO: lisää oven avautumiselle ääni
                ovi.Destroy();
            }
        }


        /// <summary>
        /// Pelaaja törmää johonkin ja menettää yhden HP:n
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka törmäsi</param>
        /// <param name="kohde">Olio, johon pelaaja törmäsi</param>
        private void Pelaaja1Vahingoittui(PhysicsObject pelaaja, PhysicsObject kohde)
        {
            pelaaja1HP.Value -= 1;
            // TODO: jos HP <= 0, peli päättyy
        }


        /// <summary>
        /// Pelaaja törmää johonkin ja menettää yhden HP:n
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka törmäsi</param>
        /// <param name="kohde">Olio, johon pelaaja törmäsi</param>
        private void Pelaaja2Vahingoittui(PhysicsObject pelaaja, PhysicsObject kohde)
        {
            pelaaja2HP.Value -= 1;
            // TODO: jos HP <= 0, peli päättyy
        }
    }
}