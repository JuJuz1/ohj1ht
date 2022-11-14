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
        /// <summary>
        /// Esitellään oliot, jotka tarvitaan attribuutteina
        /// </summary>
        private PlatformCharacter pelaaja1;
        private PlatformCharacter pelaaja2;
        private PhysicsObject ovi;
        private PhysicsObject aarre;
        private GameObject ovenPainike;
        private GameObject hissinPainike;
        private PhysicsObject hissi;
        private PhysicsObject exit;

        /// <summary>
        /// Esitellään laskurit
        /// </summary>
        private IntMeter pelaaja1HP;
        private IntMeter pelaaja2HP;
        private IntMeter aarteet;

        /// <summary>
        /// Määritellään pelikentän luomiseen ja pelaamiseen tarvittavat lvuut/muuttujat
        /// </summary>
        private readonly int tileWidth = 80;
        private readonly int tileHeight = 80;
        private readonly int maxKenttaNro = 5;
        private int kenttaNro = 1;
        private int HP1 = 3;
        private int HP2 = 3;
        private int HPvihu = 5;
        private int pisteet = 0;

        /// <summary>
        /// Ladataan pelissä tarvittavat kuvat
        /// </summary>
        private readonly Image luolanKuva = LoadImage("luola");
        private readonly Image seinanKuva = LoadImage("seina1");
        private readonly Image seinanKuva2 = LoadImage("seina2");
        private readonly Image piikinKuva = LoadImage("piikki");
        private readonly Image pelaaja1Kuva = LoadImage("pelaaja1");
        private readonly Image pelaaja2Kuva = LoadImage("pelaaja2");

        /// <summary>
        /// Peli aloitetaan ensimmäisellä kentällä
        /// </summary>
        public override void Begin()
        {
            LuoAlkuvalikko();

            Gravity = new Vector(0, -1000);
        }

        /// <summary>
        /// Luodaan peliin alkuvalikko
        /// </summary>
        private void LuoAlkuvalikko()
        {
            ClearAll();

            MultiSelectWindow alkuvalikko = new MultiSelectWindow("Alkuvalikko", "Aloita peli", "Valitse kenttä", "Lopeta");
            Add(alkuvalikko);

            alkuvalikko.AddItemHandler(0, AloitaPeli);
            alkuvalikko.AddItemHandler(1, LuoKenttavalikko);
            alkuvalikko.AddItemHandler(2, ConfirmExit, LuoAlkuvalikko);
        }

        /// <summary>
        /// Aloitetaan peli ensimmäisestä kentästä
        /// </summary>
        private void AloitaPeli()
        {
            LuoKentta(1);
        }

        /// <summary>
        /// Luodaan valikko, josta voi valita mistä kentästä aloitetaan
        /// </summary>
        private void LuoKenttavalikko()
        {
            MultiSelectWindow kenttavalikko = new MultiSelectWindow("Valitse kenttä", "Kenttä 1", "Kenttä 2", "Kenttä 3", "Takaisin");
            Add(kenttavalikko);

            kenttavalikko.AddItemHandler(0, LuoKentta, 1);
            kenttavalikko.AddItemHandler(1, LuoKentta, 2);
            kenttavalikko.AddItemHandler(2, LuoKentta, 3);
            kenttavalikko.AddItemHandler(3, LuoAlkuvalikko);
        }

        /// <summary>
        /// Luodaan kenttä
        /// </summary>
        /// <param name="nro">Luotavan kentän numero</param>
        private void LuoKentta(int nro)
        {
            kenttaNro = nro;
            ClearAll();

            if (kenttaNro > maxKenttaNro) PeliLoppuu();
            else KenttaTiedostosta("kentta" + kenttaNro);

            LuoMuut(); // Luodaan laskurit, collisionhandlerit, ohjaimet, kentän reunat ja zoomataan kamera kenttään
        }

        // TODO: Lisää kenttiä ja lopetus

        /// <summary>
        /// Luodaan valikko, joka aukeaa Escape painettaessa kentän ollessa käynnissä
        /// </summary>
        private void LuoPelivalikko()
        {
            MultiSelectWindow pelivalikko = new MultiSelectWindow("Valikko", "Jatka peliä", "Alkuvalikko", "Lopeta");
            Add(pelivalikko);

            pelivalikko.AddItemHandler(0, SuljeValikko, pelivalikko);
            pelivalikko.AddItemHandler(1, VahvistaAlkuvalikkoon);
            pelivalikko.AddItemHandler(2, ConfirmExit, LuoAlkuvalikko);
        }

        /// <summary>
        /// Suljetaan valikko
        /// </summary>
        private void SuljeValikko(MultiSelectWindow valikko)
        {
            valikko.Destroy();         
        }

        /// <summary>
        /// Kysyy pelaajalta haluaako hän mennä takaisin alkuvalikkoon.
        /// Jos kyllä, mennään alkuvalikkoon.
        /// </summary>
        private void VahvistaAlkuvalikkoon()
        {
            YesNoWindow vahvistusIkkuna = new YesNoWindow("Menetät edistymisesi pelissä, jos palaat takaisin alkuvalikkoon.\nHaluatko varmasti palata takaisin alkuvalikkoon?");
            vahvistusIkkuna.Yes += delegate { NollaaLaskurit(); LuoAlkuvalikko(); };
                Add(vahvistusIkkuna);
        }

        /// <summary>
        /// Luodaan kenttä tekstitiedostosta
        /// </summary>
        /// <param name="KenttaTiedosto">Tiedosto, joka luetaan</param>
        private void KenttaTiedostosta(string KenttaTiedosto)
        {
            TileMap kentta = TileMap.FromLevelAsset(KenttaTiedosto);
            Level.Background.Image = luolanKuva;
            kentta.SetTileMethod('X', LuoPalikka);
            kentta.SetTileMethod('Z', LuoPalikka2);
            kentta.SetTileMethod('A', LuoPiikki);
            kentta.SetTileMethod('V', LuoLaser);
            kentta.SetTileMethod('T', LuoAarre);
            kentta.SetTileMethod('D', LuoOvi);
            kentta.SetTileMethod('B', LuoOvenPainike);
            kentta.SetTileMethod('b', LuoHissinPainike);
            kentta.SetTileMethod('H', LuoHissi);
            kentta.SetTileMethod('1', LuoPelaaja1, this);
            kentta.SetTileMethod('2', LuoPelaaja2, this);
            kentta.SetTileMethod('*', LuoVihollinen);
            kentta.SetTileMethod('E', LuoExit);
            // kentta.SetTileMethod('K', LuoExit);

            kentta.Execute(tileWidth, tileHeight); // Luodaan kenttä
        }

        /// <summary>
        /// Luodaan laskurit, collisionhandlerit, ohjaimet, kentän reunat ja zoomataan kamera kenttään
        /// </summary>
        private void LuoMuut()
        {
            LuoHPLaskuri1(HP1);
            LuoHPLaskuri2(HP2);
            LuoPisteLaskuri(pisteet);

            Camera.ZoomToLevel();
            Level.CreateBorders();

            AddCollisionHandler(pelaaja1, "piikki", Pelaaja1Vahingoittui);
            AddCollisionHandler(pelaaja2, "piikki", Pelaaja2Vahingoittui);
            AddCollisionHandler(pelaaja1, "laser", Pelaaja1Vahingoittui);
            AddCollisionHandler(pelaaja2, "laser", Pelaaja2Vahingoittui);
            AddCollisionHandler(pelaaja1, "vihu", Pelaaja1Vahingoittui);
            AddCollisionHandler(pelaaja2, "vihu", Pelaaja2Vahingoittui);
            AddCollisionHandler(pelaaja1, "easter_egg", TuhoaKohde);
            AddCollisionHandler(pelaaja2, "easter_egg", TuhoaKohde);

            LuoOhjaimet();
        }

        /// <summary>
        /// Nollataan kaikki laskurit oletusarvoihin
        /// </summary>
        private void NollaaLaskurit()
        {
            HP1 = 3;
            HP2 = 3;
            pisteet = 0;
        }

        private void LuoOhjaimet()
        {
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, LuoPelivalikko, "Avaa valikko");
            Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
            Keyboard.Listen(Key.F11, ButtonState.Pressed, VaihdaFullScreen, "Kokoruututila");
            Keyboard.Listen(Key.Y, ButtonState.Pressed, LuoKentta, "Seuraava kenttä", kenttaNro + 1);

            Keyboard.Listen(Key.A, ButtonState.Down, pelaaja1.Walk, "Pelaaja 1: Kävele vasemmalle", -180.0);
            Keyboard.Listen(Key.D, ButtonState.Down, pelaaja1.Walk, "Pelaaja 1: Kävele oikealle", 180.0);
            Keyboard.Listen(Key.W, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 1: Hyppää", pelaaja1, 600.0);
            Keyboard.Listen(Key.S, ButtonState.Pressed, KaytaObjektia, "Pelaaja 1: Paina nappia / poimi esine / käytä portaali", pelaaja1);
            Keyboard.Listen(Key.F, ButtonState.Down, AmmuAseella, "Ammu", pelaaja1);

            Keyboard.Listen(Key.Left, ButtonState.Down, pelaaja2.Walk, "Pelaaja 2: Kävele vasemmalle", -180.0);
            Keyboard.Listen(Key.Right, ButtonState.Down, pelaaja2.Walk, "Pelaaja 2: Kävele oikealle", 180.0);
            Keyboard.Listen(Key.Up, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 2: Hyppää", pelaaja2, 600.0);
            Keyboard.Listen(Key.Down, ButtonState.Pressed, KaytaObjektia, "Pelaaja 2: Paina nappia / poimi esine / käytä portaali", pelaaja2);
            Keyboard.Listen(Key.RightShift, ButtonState.Down, AmmuAseella, "Ammu", pelaaja2);
        }

        private void VaihdaFullScreen()
        {
            if (IsFullScreen) IsFullScreen = false;
            else IsFullScreen = true;
            Camera.ZoomToLevel();
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
            // if (RandomGen.NextBool()) Image.Flip(kuva);
            // if (RandomGen.NextBool()) Image.Mirror(kuva);
            Add(palikka);
        }

        private void LuoPalikka2(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject palikka2 = new PhysicsObject(leveys, korkeus);
            palikka2.Position = paikka;
            palikka2.Image = seinanKuva2;
            palikka2.IgnoresCollisionResponse = true;
            palikka2.IgnoresGravity = true;
            palikka2.Image.Scaling = ImageScaling.Nearest;
            palikka2.Tag = "easter_egg";
            Add(palikka2);
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
            PhysicsObject osoitin = PhysicsObject.CreateStaticObject(leveys * 0.375, korkeus);
            osoitin.X = paikka.X;
            osoitin.Y = paikka.Y;
            osoitin.Color = Color.Gray;
            Add(osoitin);

            PhysicsObject laser = PhysicsObject.CreateStaticObject(leveys * 0.05, korkeus * 3);
            laser.X = paikka.X;
            laser.Y = paikka.Y - korkeus * 2;
            laser.Color = Color.Maroon;
            laser.Tag = "laser";
            Add(laser);

            Timer ajastin = new Timer();
            if (kenttaNro < 2) ajastin.Interval = 1.0;
            else ajastin.Interval = 0.75;
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

            // TODO: Korjaa: pelaaja ei ota vahinkoa laserin käynnistyessä, jos pelaaja on laserin kohdalla
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
            

            hissi = new PhysicsObject(leveys * 1.99, korkeus * 0.2); // Ei voi olla static jos haluaa liikuttaa
            hissi.X = paikka.X - leveys * 0.5;
            hissi.Y = paikka.Y - korkeus * 0.6;
            hissi.Shape = Shape.Rectangle;
            hissi.Color = Color.DarkAzure;
            hissi.CanRotate = false;
            hissi.Mass = 1000000;
            hissi.IgnoresGravity = true;
            hissi.Tag = "alhaalla";
            hissi.MakeOneWay();
            Add(hissi, 1);
        }


        /// <summary>
        /// Luodaan pelaaja 1 ja ase sekä aseen lisäystä tukevat komennot
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        /// <param name="peli">Fysiikkapeli, johon pelaaja lisätään</param>
        private void LuoPelaaja1(Vector paikka, double leveys, double korkeus, PhysicsGame peli)
        {
            pelaaja1 = new Pelaaja(peli, pelaaja1Kuva, paikka, leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
            pelaaja1.Weapon = new AssaultRifle(30, 15);
            // pelaaja1.Weapon.Position = pelaaja1.Position; alunperin jo keskellä
            pelaaja1.Weapon.AmmoIgnoresGravity = true;
            pelaaja1.Weapon.CanHitOwner = false;
            pelaaja1.Weapon.FireRate = 3;
            pelaaja1.Weapon.Power.DefaultValue = 200;
            pelaaja1.Weapon.ProjectileCollision = AmmusOsui;
            // pelaaja1.Weapon.AttackSound = ?;
            pelaaja1.Weapon.IsVisible = false;
            pelaaja1.Weapon.Ammo.Value = 0;

            if (2 < kenttaNro)
            {
                pelaaja1.Weapon.IsVisible = true;
                pelaaja1.Weapon.Ammo.Value = 10000;
                MessageDisplay.TextColor = Color.Black; // TODO: Parempi paikka näille ? Ei viittis laittaa luokenttään
                MessageDisplay.Font = new Font(30);
                MessageDisplay.Add("Käytössänne on nyt aseet! Paina F1 ohjeita varten!");
                MessageDisplay.Position = new Vector(0, Screen.Top - tileHeight * 1);
            }
        }


        /// <summary>
        /// Luodaan pelaaja 2 ja ase
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        /// <param name="peli">Fysiikkapeli, johon pelaaja lisätään</param>
        private void LuoPelaaja2(Vector paikka, double leveys, double korkeus, PhysicsGame peli)
        {
            pelaaja2 = new Pelaaja(peli, pelaaja2Kuva, paikka, leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
            pelaaja2.Weapon = new AssaultRifle(30, 15);
            // pelaaja2.Weapon.Position = pelaaja2.Position; alunperin jo keskellä
            pelaaja2.Weapon.AmmoIgnoresGravity = true;
            pelaaja2.Weapon.CanHitOwner = false;
            pelaaja2.Weapon.FireRate = 3;
            pelaaja2.Weapon.Power.DefaultValue = 200;
            pelaaja2.Weapon.ProjectileCollision = AmmusOsui;
            // pelaaja2.Weapon.AttackSound = ?;
            pelaaja2.Weapon.IsVisible = false;
            pelaaja2.Weapon.Ammo.Value = 0;

            if (2 < kenttaNro)
            {
                pelaaja2.Weapon.IsVisible = true;
                pelaaja2.Weapon.Ammo.Value = 10000;
            }
        }

        /// <summary>
        /// Luodaan edestakaisin värähtelevä vihollinen, johon törmätessä menettää HP. Vihollisen voi ampua
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        private void LuoVihollinen(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject vihollinen = new PhysicsObject(leveys * 0.7, korkeus*0.7); //Shape.Heart);
            vihollinen.Tag = "vihu";
            vihollinen.X = paikka.X;
            vihollinen.Y = paikka.Y;
            vihollinen.IgnoresGravity = true;
            vihollinen.IgnoresCollisionResponse = true;
            // vihollinen.Image = ?;
            // vihollinen.Image.Scaling = ImageScaling.Nearest;
            vihollinen.Oscillate(Vector.UnitX, tileWidth * 1.15, RandomGen.NextDouble(0.5, 0.75));
            Add(vihollinen);
        }

        /// <summary>
        /// Pelaajan ammuttaessa aseella suoritetaan tämä
        /// </summary>
        /// <param name="pelaaja">pelaaja, joka ampuu</param>
        private void AmmuAseella(PlatformCharacter pelaaja)
        {
            PhysicsObject ammus = pelaaja.Weapon.Shoot();

            /*
            if (ammus == null)
            {
                MessageDisplay.Color = Color.White;
                MessageDisplay.MaxMessageCount = 1;
                MessageDisplay.MessageTime = new TimeSpan(0, 0, 3);
                MessageDisplay.Add("Jotain meni väärin");
            }
            */
        }

        /// <summary>
        /// Kun pelaaja ampuu ammuksen ja se osuu johonkin tullaan tähän aliohjelmaan
        /// </summary>
        /// <param name="ammus"></param>
        /// <param name="kohde"></param>
        private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
        {
            ammus.Destroy();

            if (kohde.Tag.ToString() == "vihu")
            {
                HPvihu--;
                if (HPvihu <= 0)
                {
                    kohde.Destroy();
                    HPvihu = 5;
                }
            }
        }

        /// <summary>
        /// Luodaan Exit-portaali, josta päästään seuraavaan kenttään
        /// </summary>
        private void LuoExit(Vector paikka, double leveys, double korkeus)
        {
                exit = PhysicsObject.CreateStaticObject(leveys * 0.75, korkeus, Shape.Circle);
                exit.Position = paikka;
                exit.Color = Color.Harlequin;
                exit.IgnoresCollisionResponse = true;
                Add(exit);
        }


        /// <summary>
        /// Luodaan pelaajan 1 hitpoint laskuri ja sen näyttö
        /// </summary>
        private void LuoHPLaskuri1(int HP1)
        {
            pelaaja1HP = new IntMeter(HP1);
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
        private void LuoHPLaskuri2(int HP2)
        {
            pelaaja2HP = new IntMeter(HP2);
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
        private void LuoPisteLaskuri(int Pisteet)
        {
            aarteet = new IntMeter(Pisteet);
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
                Timer.SingleShot(1.6, delegate { hissi.Tag = "ylhaalla"; });
            }

            else if (Math.Abs(pelaaja.X - hissinPainike.X) < tileWidth * 0.3 && Math.Abs(pelaaja.Y - hissinPainike.Y) < tileHeight * 0.3 && hissi.Tag.ToString() == "ylhaalla") // Hissiä nostetaan
            {

                hissi.MoveTo(new Vector(hissi.X, hissi.Y - tileHeight * 2), 100);
                hissinPainike.Color = Color.Red;
                hissi.Tag = "liikkeessa";
                Timer.SingleShot(1.6, delegate { hissi.Tag = "alhaalla"; });
            }

            if (Math.Abs(pelaaja.X - aarre.X) < tileWidth * 0.3 && Math.Abs(pelaaja.Y - aarre.Y) < tileHeight * 0.3) // Poimitaan aarre
            {
                aarteet.Value += 1;
                pisteet++;
                // TODO: äänet
                aarre.Destroy();
                aarre.X = Screen.Left;
                aarre.Y = Screen.Top;
            }

            if (Math.Abs(pelaaja1.X - exit.X) < tileWidth * 0.5 && Math.Abs(pelaaja1.Y - exit.Y) < tileHeight * 0.5
             && Math.Abs(pelaaja2.X - exit.X) < tileWidth * 0.5 && Math.Abs(pelaaja2.Y - exit.Y) < tileHeight * 0.5) // Käytetään portaali
            {
               kenttaNro++;
               LuoKentta(kenttaNro);
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
            HP1--;
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
            HP2--;
            if (pelaaja2HP <= 0) PeliLoppuu();
        }

        /// <summary>
        /// Tuhoaa törmäyksen kohteen
        /// </summary>
        /// <param name="tormaaja">Objekti, joka törmäsi</param>
        /// <param name="kohde">Törmäyksen kohde</param>
        private void TuhoaKohde(PhysicsObject tormaaja, PhysicsObject kohde)
        {
            kohde.Destroy();
        }

        /// <summary>
        /// Peli loppuu
        /// </summary>
        private void PeliLoppuu()
        {
        ClearAll();
        ConfirmExit(LuoAlkuvalikko);
        NollaaLaskurit();
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