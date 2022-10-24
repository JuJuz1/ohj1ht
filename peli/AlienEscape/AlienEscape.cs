using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Transactions;

namespace AlienEscape
{
    public class AlienEscape : PhysicsGame
    {
        // TODO: Kentän lataaminen tekstitiedostosta
        private static readonly String[] kentta1 = {
            "XXXXXXXXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXXXXXXXX",
            "XXXXXXXX          EX",
            "XXXXVX        b XXXX",
            "X           XXXXXXXX",
            "X T       H    XXXXX",
            "XXXXXXXXX  XXX     X",
            "XXXXX        XXXX  X",
            "X    BXXX  X X    XX",
            "X   XXX   XX     XXX",
            "X12 D   XXXXXXX XXXX",
            "XXXXXXXXXXXXXXXAXXXX",
            };
        /// <summary>
        /// Esitellään oliot, jotka tarvitaan attribuutteina
        /// </summary>
        private static PlatformCharacter pelaaja1;
        private static PlatformCharacter pelaaja2;
        private static PhysicsObject ovi;
        private static PhysicsObject aarre;
        private static GameObject ovenPainike;
        private static GameObject hissinPainike;
        private static PhysicsObject hissi;
        private static PhysicsObject exit;

        /// <summary>
        /// Esitellään laskurit
        /// </summary>
        private static IntMeter pelaaja1HP;
        private static IntMeter pelaaja2HP;
        private static IntMeter aarteet;

        /// <summary>
        /// Määritellään pelikentän yhden ruudun leveys ja korkeus
        /// </summary>
        private static readonly int tileWidth = 1600 / kentta1[0].Length;
        private static readonly int tileHeight = 960 / kentta1.Length;

        /// <summary>
        /// Ladataan pelissä tarvittavat kuvat
        /// </summary>
        private static readonly Image luolanKuva = LoadImage("luola");
        private static readonly Image seinanKuva = LoadImage("seina1");
        private static readonly Image piikinKuva = LoadImage("piikki");
        private static readonly Image pelaaja1Kuva = LoadImage("pelaaja1");
        private static readonly Image pelaaja2Kuva = LoadImage("pelaaja2");

        /// <summary>
        /// Peli aloitetaan
        /// </summary>
        public override void Begin()
        {
            LuoKentta();
            Gravity = new Vector(0, -1000);

            Level.CreateBorders();
            Camera.ZoomToLevel();

            LuoHPLaskuri1();
            LuoHPLaskuri2();
            LuoPisteLaskuri();
            AddCollisionHandler(pelaaja1, "piikki", Pelaaja1Vahingoittui);
            AddCollisionHandler(pelaaja2, "piikki", Pelaaja2Vahingoittui);
            AddCollisionHandler(pelaaja1, "laser", Pelaaja1Vahingoittui);
            AddCollisionHandler(pelaaja2, "laser", Pelaaja2Vahingoittui);

            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
            Keyboard.Listen(Key.F11, ButtonState.Pressed, VaihdaFullScreen, "Kokoruututila");

            // TODO: Ohjaimien ryhmittely?
            Keyboard.Listen(Key.A, ButtonState.Down, pelaaja1.Walk, "Pelaaja 1: Kävele vasemmalle", -180.0);
            Keyboard.Listen(Key.D, ButtonState.Down, pelaaja1.Walk, "Pelaaja 1: Kävele oikealle", 180.0);
            Keyboard.Listen(Key.W, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 1: Hyppää", pelaaja1, 600.0);
            Keyboard.Listen(Key.S, ButtonState.Pressed, KaytaObjektia, "Pelaaja 1: Paina nappia / poimi esine / käytä portaali", pelaaja1);

            Keyboard.Listen(Key.Left, ButtonState.Down, pelaaja2.Walk, "Pelaaja 2: Kävele vasemmalle", -180.0);
            Keyboard.Listen(Key.Right, ButtonState.Down, pelaaja2.Walk, "Pelaaja 2: Kävele oikealle", 180.0);
            Keyboard.Listen(Key.Up, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 2: Hyppää", pelaaja2, 600.0);
            Keyboard.Listen(Key.Down, ButtonState.Pressed, KaytaObjektia, "Pelaaja 2: Paina nappia / poimi esine / käytä portaali", pelaaja2);

            // TODO: Tarvitaanko 2 seuraavaa riviä mihinkään, voiko ne poistaa?
            // Image piikki1 = LoadImage("piikki.png");
            // Shape piikki2 = Shape.FromImage(piikki1);
        }


        private void VaihdaFullScreen()
        {
            if (IsFullScreen) IsFullScreen = false;
            else IsFullScreen = true;
            Camera.ZoomToLevel();
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
            kentta.SetTileMethod('V', LuoLaser);
            kentta.SetTileMethod('T', LuoAarre);
            kentta.SetTileMethod('D', LuoOvi);
            kentta.SetTileMethod('B', LuoOvenPainike);
            kentta.SetTileMethod('b', LuoHissinPainike);
            kentta.SetTileMethod('H', LuoHissi);
            kentta.SetTileMethod('1', LuoPelaaja1, this);
            kentta.SetTileMethod('2', LuoPelaaja2, this);
            // TODO: kentta.SetTileMethod('*', LuoVihollinen);
            kentta.SetTileMethod('E', LuoExit);

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
            palikka.Image.Scaling = ImageScaling.Nearest;
            palikka.CollisionIgnoreGroup = 1;
            // if (RandomGen.NextBool()) Image.Flip(kuva);
            // if (RandomGen.NextBool()) Image.Mirror(kuva);
            // palikka.Image = kuva;
            Add(palikka);
        }


        /// <summary>
        /// Luodaan peliin piikki
        /// </summary>
        /// <param name="paikka">Piste, johon piikki luodaan</param>
        /// <param name="leveys">1 ruudun leveys</param>
        /// <param name="korkeus">1 ruudun korkeus</param>
        private void LuoPiikki(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject piikki = PhysicsObject.CreateStaticObject(leveys * 1.5, korkeus);
            piikki.Image = piikinKuva;
            piikki.Position = paikka;
            piikki.Tag = "piikki";
            Add(piikki);
        }

        /// <summary>
        /// Luodaan laser, joka kytkeytyy päälle ja pois tietyn ajanjakson välein
        /// </summary>
        /// <param name="paikka"></param>
        /// <param name="leveys"></param>
        /// <param name="korkeus"></param>
        private void LuoLaser(Vector paikka, double leveys, double korkeus)
        {
            GameObject osoitin = new GameObject(leveys * 0.375, korkeus);
            osoitin.X = paikka.X;
            osoitin.Y = paikka.Y;
            osoitin.Color = Color.Gray;
            Add(osoitin);

            PhysicsObject laser = PhysicsObject.CreateStaticObject(leveys * 0.05, korkeus * 2);
            laser.X = paikka.X;
            laser.Y = paikka.Y - korkeus * 1.5;
            laser.Color = Color.Maroon;
            laser.Tag = "laser";
            Add(laser);

            Timer ajastin = new Timer();
            ajastin.Interval = 1.0;
            ajastin.Timeout += delegate
            {
                if (laser.Tag.ToString() == "laser") // CollisionHandler käsittelee pelaajan ja laserin välisen törmäyksen vaan, jos Tag = "laser".
                {
                    laser.IsVisible = false; laser.IgnoresCollisionResponse = true; laser.Tag = ""; // Jos Tag = "laser", muutetaan näkymättömäksi, poistetaan törmäykset ja tyhjennetään tagi
                }
                else
                {
                    laser.IsVisible = true; laser.IgnoresCollisionResponse = false; laser.Tag = "laser"; // Jos Tag != "laser", muutetaan näkyväksi, otetaan törmäykset käyttöön ja vaihdetaan Tagiksi "laser"
                }
            };
            ajastin.Start();
        }

        /// <summary>
        /// Luodaan aarre, jonka pelaaja(t) voi kerätä
        /// </summary>
        /// <param name="paikka"></param>
        /// <param name="leveys"></param>
        /// <param name="korkeus"></param>
        private void LuoAarre(Vector paikka, double leveys, double korkeus)
        {
            aarre = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Star);
            aarre.Position = paikka;
            aarre.IgnoresCollisionResponse = true;
            // TODO: aarre.Image = ?;
            aarre.Color = Color.Gold;
            Add(aarre);
        }


        /// <summary>
        /// Luodaan peliin painikkeella avattava ovi
        /// </summary>
        /// <param name="paikka">Piste, johon ovi luodaan</param>
        /// <param name="leveys">1 ruudun leveys</param>
        /// <param name="korkeus">1 ruudun korkeus</param>
        private void LuoOvi(Vector paikka, double leveys, double korkeus)
        {
            ovi = PhysicsObject.CreateStaticObject(leveys * 0.6, korkeus);
            // TODO: ovi.Image = ovenKuva;
            ovi.Position = paikka;
            ovi.Shape = Shape.Rectangle;
            ovi.Color = Color.Brown;
            Add(ovi);
        }


        /// <summary>
        /// Luodaan peliin painike, jolla saa avattua oven
        /// </summary>
        /// <param name="paikka">Piste, johon oven painike luodaan</param>
        /// <param name="leveys">1 ruudun leveys</param>
        /// <param name="korkeus">1 ruudun korkeus</param>
        private void LuoOvenPainike(Vector paikka, double leveys, double korkeus)
        {
            ovenPainike = new GameObject(leveys * 0.2, korkeus * 0.2);
            // TODO: ovenPainike.Image = painikkeenKuva;
            ovenPainike.Position = paikka;
            ovenPainike.Shape = Shape.Rectangle;
            ovenPainike.Color = Color.Red;
            Add(ovenPainike);
        }
        
        /// <summary>
        /// Luodaan hissille painike
        /// </summary>
        /// <param name="paikka"></param>
        /// <param name="leveys"></param>
        /// <param name="korkeus"></param>
        private void LuoHissinPainike(Vector paikka, double leveys, double korkeus)
        {
            hissinPainike = new GameObject(leveys * 0.2, korkeus * 0.2);
            // TODO: hissinPainike.Image = jotain;
            hissinPainike.Position = paikka;
            hissinPainike.Shape = Shape.Rectangle;
            hissinPainike.Color = Color.Red;
            Add(hissinPainike);
        }

        /// <summary>
        /// Luodaan peliin 2 ruutua leveä hissi, joka voi liikkua ylös ja alas
        /// </summary>
        /// <param name="paikka">Piste, johon halutaan hissin vasemman laidan tulevan</param>
        /// <param name="leveys">1 ruudun leveys</param>
        /// <param name="korkeus">1 ruudun korkeus</param>
        private void LuoHissi(Vector paikka, double leveys, double korkeus)
        {
            GameObject hissikuilu = new GameObject(leveys * 0.5, korkeus * 2);
            hissikuilu.Y = paikka.Y + korkeus * 0.5;
            hissikuilu.X = paikka.X - leveys * 0.5;
            hissikuilu.Shape = Shape.Rectangle;
            hissikuilu.Color = Color.AshGray;
            Add(hissikuilu);
            

            hissi = new PhysicsObject(leveys * 2, korkeus * 0.2); // Ei voi olla static jos haluaa liikuttaa
            hissi.X = paikka.X - leveys * 0.5;
            hissi.Y = paikka.Y - korkeus * 0.6;
            hissi.Shape = Shape.Rectangle;
            hissi.Color = Color.DarkAzure;
            hissi.CanRotate = false;
            hissi.Mass = 1000000;
            hissi.IgnoresGravity = true;
            hissi.CollisionIgnoreGroup = 1;
            hissi.Tag = "alhaalla";
            hissi.MakeOneWay();
            Add(hissi, 1);
        }


        /// <summary>
        /// Luodaan pelaaja 1
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        /// <param name="peli">Fysiikkapeli, johon pelaaja lisätään</param>
        private void LuoPelaaja1(Vector paikka, double leveys, double korkeus, PhysicsGame peli)
        {
            pelaaja1 = new Pelaaja(peli, pelaaja1Kuva, paikka, leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
        }


        /// <summary>
        /// Luodaan pelaaja 2
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        /// /// <param name="peli">Fysiikkapeli, johon pelaaja lisätään</param>
        private void LuoPelaaja2(Vector paikka, double leveys, double korkeus, PhysicsGame peli)
        {
            pelaaja2 = new Pelaaja(peli, pelaaja2Kuva, paikka, leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
        }

        /// <summary>
        /// Luodaan Exit-portaali, josta päästään seuraavaan kenttään
        /// </summary>
        private void LuoExit(Vector paikka, double leveys, double korkeus)
        {
            exit = PhysicsObject.CreateStaticObject(leveys*0.75, korkeus, Shape.Circle);
            exit.Position = paikka;
            exit.Color = Color.Harlequin;
            exit.IgnoresCollisionResponse = true;
            Add(exit);

        }


        /// <summary>
        /// Luodaan pelaajan 1 hitpoint laskuri ja sen näyttö
        /// </summary>
        private void LuoHPLaskuri1()
        {
            pelaaja1HP = new IntMeter(3);
            pelaaja1HP.MinValue = 0;
            Label nayttoHP1 = new Label();
            nayttoHP1.X = Screen.Left + 100;
            nayttoHP1.Y = Screen.Top - 50;
            nayttoHP1.TextColor = Color.White;
            nayttoHP1.Color = Color.Red;
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
            nayttoHP2.X = Screen.Left + 200;
            nayttoHP2.Y = Screen.Top - 50;
            nayttoHP2.TextColor = Color.White;
            nayttoHP2.Color = Color.Blue;
            nayttoHP2.IntFormatString = " HP = {0:D1} ";
            nayttoHP2.BindTo(pelaaja2HP);
            Add(nayttoHP2);
        }

        /// <summary>
        /// Luodaan kerättyjen aarteiden laskuri
        /// </summary>
        private void LuoPisteLaskuri()
        {
            aarteet = new IntMeter(0);
            aarteet.MaxValue = 5;
            Label nayttoAarteet = new Label();
            nayttoAarteet.X = Screen.Left + 320;
            nayttoAarteet.Y = Screen.Top - 50;
            nayttoAarteet.TextColor = Color.Black;
            nayttoAarteet.Color = Color.Gold;
            nayttoAarteet.IntFormatString = " Aarteet = {0:D1} ";
            nayttoAarteet.BindTo(aarteet);
            Add(nayttoAarteet);
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
        /// Pelaaja käyttää lähellä olevaa objektia, kuten painaa nappia tai poimii maasta aarteen
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka yrittää käyttää lähellä olevaa objektia</param>
        private void KaytaObjektia(PhysicsObject pelaaja)
        {
            if (Math.Abs(pelaaja.X - ovenPainike.X) < tileWidth * 0.3 && Math.Abs(pelaaja.Y - ovenPainike.Y) < tileHeight * 0.3) // Avataan ovi
            {
                ovenPainike.Color = Color.Green;
                // TODO: lisää oven avautumiselle ääni
                ovi.Destroy();
            }

            if (Math.Abs(pelaaja.X - hissinPainike.X) < tileWidth * 0.3 && Math.Abs(pelaaja.Y - hissinPainike.Y) < tileHeight * 0.3 && hissi.Tag.ToString() == "alhaalla") // Hissiä nostetaan
            {
                
                hissi.MoveTo(new Vector(hissi.X, hissi.Y+tileHeight*2), 100);
                hissinPainike.Color = Color.Purple;
                hissi.Tag = "liikkeessa";
                Timer.SingleShot(1.5, delegate { hissi.Tag = "ylhaalla"; });
            }

            else if (Math.Abs(pelaaja.X - hissinPainike.X) < tileWidth * 0.3 && Math.Abs(pelaaja.Y - hissinPainike.Y) < tileHeight * 0.3 && hissi.Tag.ToString() == "ylhaalla") // Hissiä nostetaan
            {

                hissi.MoveTo(new Vector(hissi.X, hissi.Y - tileHeight * 2), 100);
                hissinPainike.Color = Color.Red;
                hissi.Tag = "liikkeessa";
                Timer.SingleShot(1.5, delegate { hissi.Tag = "alhaalla"; });
            }

            if (Math.Abs(pelaaja.X - aarre.X) < tileWidth * 0.3 && Math.Abs(pelaaja.Y - aarre.Y) < tileHeight * 0.3) // Poimitaan aarre
            {
                aarteet.Value += 1;
                // TODO: äänet
                aarre.Destroy();
                aarre.X = Screen.Left;
                aarre.Y = Screen.Top;
            }

            if (Math.Abs(pelaaja1.X - exit.X) < tileWidth * 0.5 && Math.Abs(pelaaja1.Y - exit.Y) < tileHeight * 0.5
             && Math.Abs(pelaaja2.X - exit.X) < tileWidth * 0.5 && Math.Abs(pelaaja2.Y - exit.Y) < tileHeight * 0.5) // Käytetään portaali
            {
                PeliLoppuu();
            }
        }

        /// <summary>
        /// Pelaaja 1 törmää johonkin ja menettää yhden HP:n
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka törmäsi</param>
        /// <param name="kohde">Olio, johon pelaaja törmäsi</param>
        private void Pelaaja1Vahingoittui(PhysicsObject pelaaja, PhysicsObject kohde)
        {
            pelaaja1HP.Value -= 1;
            if (pelaaja1HP <= 0) PeliLoppuu();
        }

        /// <summary>
        /// Pelaaja 2 törmää johonkin ja menettää yhden HP:n
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka törmäsi</param>
        /// <param name="kohde">Olio, johon pelaaja törmäsi</param>
        private void Pelaaja2Vahingoittui(PhysicsObject pelaaja, PhysicsObject kohde)
        {
            pelaaja2HP.Value -= 1;
            if (pelaaja2HP <= 0) PeliLoppuu();
        }


        /// <summary>
        /// Peli loppuu
        /// </summary>
        private void PeliLoppuu()
        {
        ClearAll();
        ConfirmExit();
        // TODO: äänet, tekstiä, aloita alusta-nappi yms.
        }
    }

    public class Pelaaja : PlatformCharacter
    {
        public Pelaaja(PhysicsGame peli, Image kuva, Vector paikka, double leveys, double korkeus, Shape muoto): base(leveys, korkeus, muoto)
        {
            this.Position = paikka;
            this.Image = kuva;
            this.Image.Scaling = ImageScaling.Nearest;
            peli.Add(this);
        }


    }
}