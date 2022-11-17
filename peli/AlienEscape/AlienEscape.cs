using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Transactions;

namespace AlienEscape
{
    /// <summary>
    /// Luodaan pelin luokka
    /// </summary>
    /// @author Juuso Piippo & Elias Lehtinen
    /// @version 16.11.2022
    public class AlienEscape : PhysicsGame
    {
        /// <summary>
        /// Esitellään oliot, jotka tarvitaan attribuutteina
        /// </summary>
        private PlatformCharacter pelaaja1;
        private PlatformCharacter pelaaja2;
        private List<PhysicsObject> ovetLista = new List<PhysicsObject>();
        private List<PhysicsObject> aarteetLista = new List<PhysicsObject>();
        private List<GameObject> painikkeetLista = new List<GameObject>();
        private GameObject vipu;
        private PhysicsObject hissi;
        private PhysicsObject exit;
        private PhysicsObject aselaatikko;
        private PhysicsObject putoavaPommi;

        /// <summary>
        /// Esitellään laskurit
        /// </summary>
        private IntMeter pelaaja1HP;
        private IntMeter pelaaja2HP;
        private IntMeter aarteet;

        /// <summary>
        /// Määritellään pelikentän luomiseen ja pelaamiseen tarvittavat lvuut/muuttujat
        /// </summary>
        private const int tileWidth = 80;
        private const int tileHeight = 80;
        private const int maxKenttaNro = 5; // TODO: Muutetaan maxKenttaNro sitä mukaa, kun lisätään kenttiä
        private int kenttaNro = 1;
        private int HP1 = 3;
        private int HP2 = 3;
        // private int HPvihu = 5;
        private int pisteet = 0;
        private Label nayttoHP1;
        private Label nayttoHP2;
        private Label nayttoAarteet;

        /// <summary>
        /// Ladataan pelissä tarvittavat kuvat
        /// </summary>
        private readonly Image luolanKuva = LoadImage("luola");
        private readonly Image seinanKuva = LoadImage("seina1");
        private readonly Image seinanKuva2 = LoadImage("seina2");
        private readonly Image piikinKuva = LoadImage("piikki");
        private readonly Image ovenKuva = LoadImage("ovi");
        private readonly Image napinKuvaVih = LoadImage("nappi_vihrea");
        private readonly Image napinKuvaPun = LoadImage("nappi_punainen");
        private readonly Image vipuYlosKuva = LoadImage("vipu_ylos");
        private readonly Image vipuAlasKuva = LoadImage("vipu_alas");
        private readonly Image laserseinanKuva = LoadImage("laserpalikka");
        private readonly Image hissikuilunKuva = LoadImage("hissikuilu");
        private readonly Image hissiPaikallaanKuva = LoadImage("hissi_paikallaan");
        private readonly Image hissiLiikkeessaKuva = LoadImage("hissi_liikkeessa");
        private readonly Image aarteenKuva = LoadImage("aarre");
        private readonly Image[] oviLiikkuuKuvat = LoadImages("ovi2", "ovi3", "ovi4", "ovi5", "ovi6", "ovi7", "ovi8", "ovi9", "ovi10");
        private readonly Image pelaaja1Kuva = LoadImage("pelaaja1_1");
        private readonly Image pelaaja1Hyppy = LoadImage("pelaaja1_hyppy");
        private readonly Image[] pelaaja1Kavely = LoadImages("pelaaja1_2", "pelaaja1_4", "pelaaja1_3");
        private readonly Image pelaaja1KuvaAse = LoadImage("pelaaja1_1_A");
        private readonly Image pelaaja1HyppyAse = LoadImage("pelaaja1_hyppy_A");
        private readonly Image[] pelaaja1KavelyAse = LoadImages("pelaaja1_2_A", "pelaaja1_4_A", "pelaaja1_3_A");
        private readonly Image pelaaja2Kuva = LoadImage("pelaaja2_1");
        private readonly Image pelaaja2Hyppy = LoadImage("pelaaja2_hyppy");
        private readonly Image[] pelaaja2Kavely = LoadImages("pelaaja2_2", "pelaaja2_4", "pelaaja2_3");
        private readonly Image pelaaja2KuvaAse = LoadImage("pelaaja2_1_A");
        private readonly Image pelaaja2HyppyAse = LoadImage("pelaaja2_hyppy_A");
        private readonly Image[] pelaaja2KavelyAse = LoadImages("pelaaja2_2_A", "pelaaja2_4_A", "pelaaja2_3_A");
        private readonly Image alienKuva = LoadImage("alien1");
        private readonly Image[] alienKavely = LoadImages("alien3", "alien1", "alien2", "alien1");

        /// <summary>
        /// Ladataan pelissä tarvittavat äänet
        /// </summary>
        private readonly SoundEffect aktivoiAseAani = LoadSoundEffect("aktivoiAse");
        private readonly SoundEffect osuiAani = LoadSoundEffect("osui");
        private readonly SoundEffect[] sattuiAanet = LoadSoundEffects("aiai", "aiai2", "auts");
        private readonly SoundEffect aarreAani = LoadSoundEffect("aarre1");
        private readonly SoundEffect[] hyppyAanet = LoadSoundEffects("hyppy1", "hyppy2", "hyppy3", "hyppy4");
        private readonly SoundEffect nappiAani = LoadSoundEffect("nappi1");
        private readonly SoundEffect oviAani = LoadSoundEffect("ovi");
        private readonly SoundEffect vipuAani = LoadSoundEffect("vipu");
        private readonly SoundEffect kavelyAani = LoadSoundEffect("kavely");

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

            MediaPlayer.Stop();
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
            MultiSelectWindow kenttavalikko = new MultiSelectWindow("Valitse kenttä", "Kenttä 1", "Kenttä 2", "Kenttä 3", "Kenttä 4", "Kenttä 5", "Takaisin");
            Add(kenttavalikko);

            kenttavalikko.AddItemHandler(0, LuoKentta, 1);
            kenttavalikko.AddItemHandler(1, LuoKentta, 2);
            kenttavalikko.AddItemHandler(2, LuoKentta, 3);
            kenttavalikko.AddItemHandler(3, LuoKentta, 4);
            kenttavalikko.AddItemHandler(4, LuoKentta, 5);
            kenttavalikko.AddItemHandler(5, LuoAlkuvalikko);
        }


        /// <summary>
        /// Luodaan kenttä
        /// </summary>
        /// <param name="nro">Luotavan kentän numero</param>
        private void LuoKentta(int nro)
        {
            kenttaNro = nro;
            ClearAll();
            // TODO: tähän kaikki oliolistat
            ovetLista.Clear();
            painikkeetLista.Clear();
            aarteetLista.Clear();

            if (kenttaNro > maxKenttaNro) PeliLoppuu();
            else KenttaTiedostosta("kentta" + kenttaNro);

            LuoMuut(); // Luodaan laskurit, collisionhandlerit, ohjaimet, kentän reunat ja zoomataan kamera kenttään

            MediaPlayer.Play("taustamusiikki1"); // Käynnistetään taustamusiikki
            MediaPlayer.IsRepeating = true;
        }

        // TODO: Lisää kenttiä ja lopetus, muista muuttaa maxKenttaNro arvoa, kun lisätään kenttiä


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
            kentta.SetTileMethod('b', LuoVipu);
            kentta.SetTileMethod('H', LuoHissi);
            kentta.SetTileMethod('1', LuoPelaaja1);
            kentta.SetTileMethod('2', LuoPelaaja2);
            kentta.SetTileMethod('*', LuoVihollinen);
            kentta.SetTileMethod('E', LuoExit);
            kentta.SetTileMethod('L', LuoAseLaatikko);

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

            string[] vahingoittavat = { "piikki", "laser", "vihu5", "vihu4", "vihu3", "vihu2", "vihu1", "putoava" };

            foreach (string tag in vahingoittavat)
            {
                AddCollisionHandler(pelaaja1, tag, Pelaaja1Vahingoittui);
                AddCollisionHandler(pelaaja2, tag, Pelaaja2Vahingoittui);
            }

            AddCollisionHandler(pelaaja1, "easter_egg", TuhoaKohde);
            AddCollisionHandler(pelaaja2, "easter_egg", TuhoaKohde);

            LuoOhjaimet();

            if (kenttaNro == 5)
            {
                PudotaPommeja();
                AktivoiAseet();
            }
        }


        /// <summary>
        /// Luodaan putoavat palikat
        /// </summary>
        private void PudotaPommeja()
        {
            Timer ajastin1 = new Timer();
            ajastin1.Interval = 3.0;
            ajastin1.Timeout += delegate
            {
                putoavaPommi = new PhysicsObject(tileWidth * 0.3, tileHeight * 0.3, Shape.Circle);
                putoavaPommi.X = RandomGen.NextDouble(Screen.Left + tileWidth * 2, Screen.Right - tileWidth * 2);
                putoavaPommi.Y = Screen.Top - tileHeight * 0.5;
                putoavaPommi.Tag = "putoava";
                putoavaPommi.LifetimeLeft = TimeSpan.FromSeconds(2.0);
                Add(putoavaPommi);

                Timer.SingleShot(1.99, delegate { RajaytaPommi(); });
            };

            ajastin1.Start();
        }


        /// <summary>
        /// Tekee rajahdyksen pommille
        /// </summary>
        private void RajaytaPommi()
        {
            Explosion rajahdys = new Explosion(tileWidth);
            rajahdys.Position = putoavaPommi.Position;
            // rajahdys.Image = rajahdysKuva;
            // rajahdys.Sound = rajahdysAani;
            if (!putoavaPommi.IsDestroyed)
            { 
                Add(rajahdys);

                if (Etaisyys(pelaaja1, rajahdys) < tileWidth * 1.25)
                {
                    /*
                    HP1--;
                    pelaaja1HP.Value -= 1;
                    if (pelaaja1HP <= 0) PeliLoppuu();
                    */
                    Pelaaja1Vahingoittui(pelaaja1, putoavaPommi);
                }

                if (Etaisyys(pelaaja2, rajahdys) < tileWidth * 1.25)
                {
                    /*
                    HP2--;
                    pelaaja2HP.Value -= 1;
                    if (pelaaja2HP <= 0) PeliLoppuu();
                    */
                    Pelaaja2Vahingoittui(pelaaja2, putoavaPommi);
                }
            }
        }


        /// <summary>
        /// Rajayttaa pommin (pelaajan koskettaessa) ((väliaikainen?))
        /// </summary>
        private void RajaytaPommi2()
        {
            Explosion rajahdys = new Explosion(tileWidth);
            rajahdys.Position = putoavaPommi.Position;
            // rajahdys.Image = rajahdysKuva;
            // rajahdys.Sound = rajahdysAani;
            Add(rajahdys);
        }


        /// <summary>
        /// Muutetaan aseet näkyviksi ja mahdollistetaan niillä ampuminen
        /// </summary>
        private void AktivoiAseet()
        {
            pelaaja1.Weapon.Ammo.Value = 10000;
            pelaaja1.AnimIdle = pelaaja1KuvaAse;
            pelaaja1.AnimWalk = new Animation(pelaaja1KavelyAse);
            pelaaja1.AnimJump = pelaaja1HyppyAse;
            pelaaja1.AnimFall = pelaaja1KuvaAse;
            pelaaja1.AnimWalk.FPS = 10;
            pelaaja2.Weapon.Ammo.Value = 10000;
            pelaaja2.AnimIdle = pelaaja2KuvaAse;
            pelaaja2.AnimWalk = new Animation(pelaaja2KavelyAse);
            pelaaja2.AnimJump = pelaaja2HyppyAse;
            pelaaja2.AnimFall = pelaaja2KuvaAse;
            pelaaja2.AnimWalk.FPS = 10;
            if (kenttaNro == 4)
            {
                MessageDisplay.TextColor = Color.Black;
                MessageDisplay.Font = new Font(30);
                MessageDisplay.Add("Käytössänne on nyt aseet! Paina F1 ohjeita varten!");
                MessageDisplay.Position = new Vector(0, Screen.Top - tileHeight * 1);
                aktivoiAseAani.Play();
            }
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


        /// <summary>
        /// Luodaan peliin ohjaimet
        /// </summary>
        private void LuoOhjaimet()
        {
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, LuoPelivalikko, "Avaa valikko");
            Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
            Keyboard.Listen(Key.F11, ButtonState.Pressed, VaihdaFullScreen, "Kokoruututila");
            Keyboard.Listen(Key.M, ButtonState.Pressed, HiljennaMusiikki, "Hiljennä musiikki");
            Keyboard.Listen(Key.Y, ButtonState.Pressed, LuoKentta, "Seuraava kenttä", kenttaNro + 1);

            Keyboard.Listen(Key.A, ButtonState.Down, Kavele, "Pelaaja 1: Kävele vasemmalle", pelaaja1, -180.0);
            Keyboard.Listen(Key.D, ButtonState.Down, Kavele, "Pelaaja 1: Kävele oikealle", pelaaja1, 180.0);
            Keyboard.Listen(Key.W, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 1: Hyppää", pelaaja1, 600.0);
            Keyboard.Listen(Key.S, ButtonState.Pressed, KaytaObjektia, "Pelaaja 1: Paina nappia / poimi esine / käytä portaali", pelaaja1);
            Keyboard.Listen(Key.F, ButtonState.Down, AmmuAseella, "Pelaaja 1: Ammu", pelaaja1);

            Keyboard.Listen(Key.Left, ButtonState.Down, Kavele, "Pelaaja 2: Kävele vasemmalle", pelaaja2, -180.0);
            Keyboard.Listen(Key.Right, ButtonState.Down, Kavele, "Pelaaja 2: Kävele oikealle", pelaaja2, 180.0);
            Keyboard.Listen(Key.Up, ButtonState.Pressed, PelaajaHyppaa, "Pelaaja 2: Hyppää", pelaaja2, 600.0);
            Keyboard.Listen(Key.Down, ButtonState.Pressed, KaytaObjektia, "Pelaaja 2: Paina nappia / poimi esine / käytä portaali", pelaaja2);
            Keyboard.Listen(Key.RightShift, ButtonState.Down, AmmuAseella, "Pelaaja 2: Ammu", pelaaja2);
        }

        private void VaihdaFullScreen()
        {
            if (IsFullScreen) IsFullScreen = false;
            else IsFullScreen = true;
            Camera.ZoomToLevel();
            nayttoHP1.Position = new Vector(Screen.Left + 100, Screen.Top - 50);
            nayttoHP2.Position = new Vector(Screen.Left + 200, Screen.Top - 50);
            nayttoAarteet.Position = new Vector(Screen.Left + 320, Screen.Top - 50);
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
            palikka.Tag = "perus";
            palikka.Image.Scaling = ImageScaling.Nearest;
            // if (RandomGen.NextBool()) Image.Flip(kuva);
            // if (RandomGen.NextBool()) Image.Mirror(kuva);
            Add(palikka);
        }


        /// <summary>
        /// Luodaan läpikuljettava palikka
        /// </summary>
        /// <param name="paikka">Piste, johon palikka syntyy</param>
        /// <param name="leveys">Palikan leveys</param>
        /// <param name="korkeus">Palikan korkeus</param>
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
            piikki.Image.Scaling = ImageScaling.Nearest;
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
            PhysicsObject osoitin = PhysicsObject.CreateStaticObject(leveys, korkeus);
            osoitin.X = paikka.X;
            osoitin.Y = paikka.Y;
            osoitin.Image = laserseinanKuva;
            osoitin.Image.Scaling = ImageScaling.Nearest;
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
                    laser.IsVisible = false; 
                    laser.IgnoresCollisionResponse = true;
                    laser.Tag = "laserPois"; // Jos Tag = "laser", muutetaan näkymättömäksi, poistetaan törmäykset ja tagi "laserPois"
                }

                else
                {
                    laser.IsVisible = true;
                    laser.IgnoresCollisionResponse = false;
                    laser.Tag = "laser"; // Jos Tag != "laser", muutetaan näkyväksi, otetaan törmäykset käyttöön ja vaihdetaan Tagiksi "laser"
                    if (Math.Abs(laser.Y - pelaaja1.Y) < tileHeight * 1.5 && Math.Abs(laser.X - pelaaja1.X) < tileWidth * 0.3)
                    {
                        Pelaaja1Vahingoittui(pelaaja1, laser);
                    }

                    if (Math.Abs(laser.Y - pelaaja2.Y) < tileHeight * 1.5 && Math.Abs(laser.X - pelaaja2.X) < tileWidth * 0.3)
                    {
                        Pelaaja2Vahingoittui(pelaaja2, laser);
                    }
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
            PhysicsObject aarre = PhysicsObject.CreateStaticObject(leveys, korkeus);
            aarre.Position = paikka;
            aarre.IgnoresCollisionResponse = true;
            aarre.Image = aarteenKuva;
            aarre.Image.Scaling = ImageScaling.Nearest;
            aarre.Tag = "aarre";
            aarteetLista.Add(aarre);
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
            PhysicsObject ovi = PhysicsObject.CreateStaticObject(leveys * 0.6, korkeus);
            ovi.Image = ovenKuva;
            ovi.Image.Scaling = ImageScaling.Nearest;
            ovi.Position = paikka;
            ovi.Shape = Shape.Rectangle;
            ovi.Color = Color.Brown;
            ovi.Tag = "ovi";
            ovetLista.Add(ovi);
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
            GameObject ovenPainike = new GameObject(leveys * 0.2, korkeus * 0.2);
            ovenPainike.Image = napinKuvaPun;
            ovenPainike.Image.Scaling = ImageScaling.Nearest;
            ovenPainike.Position = paikka;
            ovenPainike.Shape = Shape.Rectangle;
            ovenPainike.Color = Color.Red;
            painikkeetLista.Add(ovenPainike);
            Add(ovenPainike);
        }
        

        /// <summary>
        /// Luodaan hissille vipu
        /// </summary>
        /// <param name="paikka"></param>
        /// <param name="leveys"></param>
        /// <param name="korkeus"></param>
        private void LuoVipu(Vector paikka, double leveys, double korkeus) // hissinPainike -> vipu (nimi vaihdettu)
        {
            vipu = new GameObject(leveys * 0.2, korkeus * 0.6);
            vipu.Image = vipuAlasKuva;
            vipu.Image.Scaling = ImageScaling.Nearest;
            vipu.Position = paikka;
            vipu.Shape = Shape.Rectangle;
            vipu.Color = Color.Red;
            Add(vipu);
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
            hissikuilu.X = paikka.X - leveys * 0.5;
            hissikuilu.Y = paikka.Y + korkeus * 0.5;
            hissikuilu.Image = hissikuilunKuva;
            hissikuilu.Image.Scaling = ImageScaling.Nearest;
            hissikuilu.Shape = Shape.Rectangle;
            hissikuilu.Color = Color.AshGray;
            Add(hissikuilu);
            

            hissi = new PhysicsObject(leveys * 1.99, korkeus * 0.2); // Ei voi olla static jos haluaa liikuttaa
            hissi.X = paikka.X - leveys * 0.5;
            hissi.Y = paikka.Y - korkeus * 0.6;
            hissi.Image = hissiPaikallaanKuva;
            hissi.Image.Scaling = ImageScaling.Nearest;
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
        private void LuoPelaaja1(Vector paikka, double leveys, double korkeus)
        {
            pelaaja1 = new PlatformCharacter(leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
            pelaaja1.Position = paikka;
            pelaaja1.Image = pelaaja1Kuva;
            pelaaja1.Image.Scaling = ImageScaling.Nearest;
            pelaaja1.AnimWalk = new Animation(pelaaja1Kavely);
            pelaaja1.AnimWalk.FPS = 10;
            pelaaja1.AnimIdle = pelaaja1Kuva;
            pelaaja1.AnimJump = pelaaja1Hyppy;
            pelaaja1.AnimFall = pelaaja1Kuva;
            pelaaja1.IgnoresExplosions = true;
            LisaaAse(pelaaja1);
            Add(pelaaja1, 2);
        }


        /// <summary>
        /// Luodaan pelaaja 2 ja ase
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        /// <param name="peli">Fysiikkapeli, johon pelaaja lisätään</param>
        private void LuoPelaaja2(Vector paikka, double leveys, double korkeus)
        {
            pelaaja2 = new PlatformCharacter(leveys * 0.5, korkeus * 0.8, Shape.Rectangle);
            pelaaja2.Position = paikka;
            pelaaja2.Image = pelaaja2Kuva;
            pelaaja2.Image.Scaling = ImageScaling.Nearest;
            pelaaja2.AnimWalk = new Animation(pelaaja2Kavely);
            pelaaja2.AnimWalk.FPS = 10;
            pelaaja2.AnimIdle = pelaaja2Kuva;
            pelaaja2.AnimJump = pelaaja2Hyppy;
            pelaaja2.AnimFall = pelaaja2Kuva;
            pelaaja2.IgnoresExplosions = true;
            LisaaAse(pelaaja2);
            Add(pelaaja2, 2);
        }


        /// <summary>
        /// Lisää pelaajalle laseraseen
        /// </summary>
        /// <param name="pelaaja">Pelaaja, jolle ase lisätään</param>
        private void LisaaAse(PlatformCharacter pelaaja)
        {
            pelaaja.Weapon = new LaserGun(30, 15);
            pelaaja.Weapon.AmmoIgnoresGravity = true;
            pelaaja.Weapon.CanHitOwner = false;
            pelaaja.Weapon.FireRate = 3;
            pelaaja.Weapon.Power.SetValue(200);
            pelaaja.Weapon.Power.DefaultValue = 200; // Nollaa DefaulValueen vasta ekan kerran jälkeen
            pelaaja.Weapon.ProjectileCollision = AmmusOsui;
            pelaaja.Weapon.IsVisible = false;
            pelaaja.Weapon.Ammo.Value = 0;
        }


        /// <summary>
        /// Luodaan edestakaisin värähtelevä vihollinen, johon törmätessä menettää HP. Vihollisen voi ampua
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        private void LuoVihollinen(Vector paikka, double leveys, double korkeus)
        {
            PhysicsObject vihollinen = new PhysicsObject(leveys * 0.7, korkeus * 0.7);
            vihollinen.Tag = "vihu5";
            vihollinen.Position = paikka;
            vihollinen.Y -= 12; // Vihujen jalat koskettavat maata
            vihollinen.IgnoresGravity = true;
            vihollinen.IgnoresCollisionResponse = true;
            vihollinen.Animation = new Animation(alienKavely);
            vihollinen.Image.Scaling = ImageScaling.Nearest;
            if (kenttaNro < 4) { vihollinen.Oscillate(Vector.UnitX, tileWidth * 0.75, RandomGen.NextDouble(0.6, 0.75)); }
            else { vihollinen.Oscillate(Vector.UnitX, tileWidth * 1.15, RandomGen.NextDouble(0.7, 0.85)); }
            Add(vihollinen);


            /*
            PlatformCharacter vihollinen = new PlatformCharacter(leveys * 0.7, korkeus * 0.7);
            polku.Add(new Vector(vihollinen.X - tileWidth, paikka.Y));
            polku.Add(new Vector(vihollinen.X, paikka.Y));
            polku.Add(new Vector(paikka.X + tileWidth, paikka.Y));
            PathFollowerBrain polkuaivot = new PathFollowerBrain();
            polkuaivot.Path = polku;
            polkuaivot.Loop = true;
            polkuaivot.Speed = 100;
            vihollinen.Brain = polkuaivot;
            */
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
            osuiAani.Play();

            var rajahdys = new Explosion(tileWidth * 0.05);
            rajahdys.Position = ammus.Position;
            Add(rajahdys);

            if (kohde.Tag.ToString() == "laserPois" || kohde.Tag.ToString() == "aarre") return;

            ammus.Destroy();

            if (kohde.Tag.ToString() == "vihu5")
            {
                kohde.Tag = "vihu4";
                return;
            }

            if (kohde.Tag.ToString() == "vihu4")
            {
                kohde.Tag = "vihu3";
                return;
            }

            if (kohde.Tag.ToString() == "vihu3")
            {
                kohde.Tag = "vihu2";
                return;
            }

            if (kohde.Tag.ToString() == "vihu2")
            {
                kohde.Tag = "vihu1";
                return;
            }

            if (kohde.Tag.ToString() == "vihu1")
            {
                kohde.Destroy();
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
        /// Luodaan aselaatikko
        /// </summary>
        /// <param name="paikka">Piste, johon pelaaja luodaan</param>
        /// <param name="leveys">1 ruudun leveys pelikentällä</param>
        /// <param name="korkeus">1 ruudun korkeus pelikentällä</param>
        private void LuoAseLaatikko(Vector paikka, double leveys, double korkeus)
        {
            aselaatikko = PhysicsObject.CreateStaticObject(leveys, korkeus * 0.5);
            aselaatikko.X = paikka.X;
            aselaatikko.Y = paikka.Y - korkeus * 0.25;
            aselaatikko.Color = Color.DarkBrown;
            // TODO: aselaatikko.Image = ??;
            // aselaatikko.Image.Scaling = ImageScaling.Nearest;
            aselaatikko.IgnoresCollisionResponse = true;
            Add(aselaatikko);
        }


        /// <summary>
        /// Luodaan pelaajan 1 hitpoint laskuri ja sen näyttö
        /// </summary>
        private void LuoHPLaskuri1(int HP1)
        {
            pelaaja1HP = new IntMeter(HP1);
            pelaaja1HP.MinValue = 0;
            nayttoHP1 = new Label();
            nayttoHP1.Position = new Vector(Screen.Left + 100, Screen.Top - 50);
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
            nayttoHP2 = new Label();
            nayttoHP2.Position = new Vector(Screen.Left + 200, Screen.Top - 50);
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
            nayttoAarteet = new Label();
            nayttoAarteet.Position = new Vector(Screen.Left + 320, Screen.Top - 50);
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
            SoundEffect hyppyAani = RandomGen.SelectOne(hyppyAanet);
            if (pelaaja.Jump(hyppaysnopeus)) hyppyAani.Play();
        }

        
        /// <summary>
        /// Olio kävelee annetulla nopeudella vaakasuunnassa
        /// </summary>
        /// <param name="olio">Olio, joka kävelee</param>
        /// <param name="v">Kävelyn nopeus, kävelee oikealle, jos positiivinen ja vasemmalle, jos negatiivinen</param>
        private void Kavele(PlatformCharacter olio, double v)
        {
            olio.Walk(v);
            if (!kavelyAani.IsPlaying) kavelyAani.Play();
        }


        /// <summary>
        /// Pelaaja käyttää lähellä olevaa objektia, kuten painaa nappia tai poimii maasta aarteen
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka yrittää käyttää lähellä olevaa objektia</param>
        private void KaytaObjektia(PhysicsObject pelaaja)
        {
            if (painikkeetLista.Count > 0)
            {
                GameObject lahinPainike = EtsiLahin(painikkeetLista, pelaaja);
                if (Etaisyys(pelaaja, lahinPainike) < tileWidth * 0.3)
                {
                    lahinPainike.Color = Color.Green;
                    nappiAani.Play();
                    if (lahinPainike.Image == napinKuvaPun) oviAani.Play();
                    lahinPainike.Image = napinKuvaVih;
                    lahinPainike.Image.Scaling = ImageScaling.Nearest;
                    EtsiLahinP(ovetLista, pelaaja).Destroy();
                }
            }

            if (vipu != null)
            {
                if ((Etaisyys(pelaaja, vipu) < tileWidth * 0.3) && hissi.Tag.ToString() == "alhaalla")
                {
                    hissi.MoveTo(new Vector(hissi.X, hissi.Y + tileHeight * 2), 100);
                    vipu.Color = Color.Purple;
                    vipu.Image = vipuYlosKuva;
                    vipu.Image.Scaling = ImageScaling.Nearest;
                    vipuAani.Play();
                    hissi.Image = hissiLiikkeessaKuva;
                    hissi.Image.Scaling = ImageScaling.Nearest;
                    hissi.Tag = "liikkeessa";
                    Timer.SingleShot(1.6, delegate { hissi.Tag = "ylhaalla"; hissi.Image = hissiPaikallaanKuva; hissi.Image.Scaling = ImageScaling.Nearest; hissi.MakeOneWay(); });
                }

                else if ((Etaisyys(pelaaja, vipu) < tileWidth * 0.3) && hissi.Tag.ToString() == "ylhaalla")
                {
                    hissi.MoveTo(new Vector(hissi.X, hissi.Y - tileHeight * 2), 100);
                    vipu.Color = Color.Red;
                    vipu.Image = vipuAlasKuva;
                    vipu.Image.Scaling = ImageScaling.Nearest;
                    vipuAani.Play();
                    hissi.Image = hissiLiikkeessaKuva;
                    hissi.Image.Scaling = ImageScaling.Nearest;
                    hissi.Tag = "liikkeessa";
                    Timer.SingleShot(1.6, delegate { hissi.Tag = "alhaalla"; hissi.Image = hissiPaikallaanKuva; hissi.Image.Scaling = ImageScaling.Nearest; hissi.MakeOneWay(); });
                }
            }

            if (aarteetLista.Count > 0)
            {
                PhysicsObject lahinAarre = EtsiLahinP(aarteetLista, pelaaja);
                if (Etaisyys(pelaaja, lahinAarre) < tileWidth * 0.5)
                {
                    aarteet.Value += 1;
                    pisteet++;
                    aarreAani.Play();
                    lahinAarre.Destroy();
                    lahinAarre.X = Screen.Left;
                    lahinAarre.Y = Screen.Top;
                }
            }

            if (exit != null)
            {
                if ((Etaisyys(pelaaja1, exit) < tileWidth * 0.5) && (Etaisyys(pelaaja2, exit) < tileWidth * 0.5))
                {
                    kenttaNro++;
                    LuoKentta(kenttaNro);
                }
            }

            if (aselaatikko != null) // Kaatuu jos ei ole
            {
                if ((Etaisyys(pelaaja, aselaatikko) < tileWidth * 0.5))
                {
                    aselaatikko.Destroy();
                    aselaatikko.X = Screen.Left;
                    aselaatikko.Y = Screen.Top;
                    AktivoiAseet();
                }
            }
        }


        /// <summary>
        /// Laskee kahden olion välisen etäisyyden
        /// </summary>
        /// <param name="o1">ensimmäinen olio</param>
        /// <param name="o2">toinen olio</param>
        /// <returns>etäisyys</returns>
        private static double Etaisyys(GameObject o1, GameObject o2)
        {
            if (o2 == null) return double.MaxValue;
            return Math.Sqrt((o1.X - o2.X) * (o1.X - o2.X) + (o1.Y - o2.Y) * (o1.Y - o2.Y)); // c = sqrt(a^2 + b^2)
        }


        /// <summary>
        /// Etsii listasta oliota lähinnä pelialueella sijaitsevan olion
        /// </summary>
        /// <param name="lista">Lista olioita</param>
        /// <param name="olio">Olio, josta etäisyydet mitataan</param>
        /// <returns>Lähin olio</returns>
        private static GameObject EtsiLahin(List<GameObject> lista, PhysicsObject olio)
        {
            GameObject lahin = new GameObject(0, 0);
            double pieninEtaisyys = double.MaxValue;
            for (int i = 0; i < lista.Count; i++)
            {
                double tamaEtaisyys = Etaisyys(olio, lista[i]);
                if (tamaEtaisyys < pieninEtaisyys)
                {
                    pieninEtaisyys = tamaEtaisyys;
                    lahin = lista[i];
                }
            }
            return lahin;
        }
        

        /// <summary>
        /// Etsii listasta oliota lähinnä pelialueella sijaitsevan olion
        /// </summary>
        /// <param name="lista">Lista olioita</param>
        /// <param name="olio">Olio, josta etäisyydet mitataan</param>
        /// <returns>Lähin olio</returns>
        private static PhysicsObject EtsiLahinP(List<PhysicsObject> lista, PhysicsObject olio)
        {
            PhysicsObject lahin = new PhysicsObject(0, 0);
            double pieninEtaisyys = double.MaxValue;
            for (int i = 0; i < lista.Count; i++)
            {
                double tamaEtaisyys = Etaisyys(olio, lista[i]);
                if (tamaEtaisyys < pieninEtaisyys)
                {
                    pieninEtaisyys = tamaEtaisyys;
                    lahin = lista[i];
                }
            }
            return lahin;
        }


        /// <summary>
        /// Pelaaja 1 törmää johonkin ja menettää yhden HP:n
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka törmäsi</param>
        /// <param name="kohde">Olio, johon pelaaja törmäsi</param>
        private void Pelaaja1Vahingoittui(PhysicsObject pelaaja, PhysicsObject kohde)
        {
            SoundEffect sattui = RandomGen.SelectOne(sattuiAanet);
            sattui.Play();

            pelaaja1HP.Value -= 1;
            HP1--;

            if (kohde.Tag.ToString() == "putoava") { kohde.Destroy(); RajaytaPommi2(); }
            if (kohde.Tag.ToString() == "laser") osuiAani.Play();

            if (pelaaja1HP <= 0) PeliLoppuu();
        }


        /// <summary>
        /// Pelaaja 2 törmää johonkin ja menettää yhden HP:n
        /// </summary>
        /// <param name="pelaaja">Pelaaja, joka törmäsi</param>
        /// <param name="kohde">Olio, johon pelaaja törmäsi</param>
        private void Pelaaja2Vahingoittui(PhysicsObject pelaaja, PhysicsObject kohde)
        {
            SoundEffect sattui = RandomGen.SelectOne(sattuiAanet);
            sattui.Play();

            pelaaja2HP.Value -= 1;
            HP2--;

            if (kohde.Tag.ToString() == "putoava") { kohde.Destroy(); RajaytaPommi2(); }
            if (kohde.Tag.ToString() == "laser") osuiAani.Play();

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
        /// Hiljentaa pelin taustamusiikin tai poistaa hiljennyksen, jos musiikki on valmiiksi hiljennetty
        /// </summary>
        private void HiljennaMusiikki()
        {
            if (MediaPlayer.IsMuted) MediaPlayer.IsMuted = false;
            else MediaPlayer.IsMuted = true;
        }


        /// <summary>
        /// Peli loppuu
        /// </summary>
        private void PeliLoppuu()
        {
        ClearAll();
        ConfirmExit(LuoAlkuvalikko); // TODO: muuta niin, että voi aloittaa kentän alusta
        NollaaLaskurit();
        MediaPlayer.Stop();
        // TODO: äänet, tekstiä, aloita alusta-nappi yms.
        }
    }

    /*
    public class Pelaaja : PlatformCharacter
    {
        public Pelaaja(PhysicsGame peli, Image kuva, Vector paikka, double leveys, double korkeus, Shape muoto): base(leveys, korkeus, muoto)
        {
            this.Position = paikka;
            this.Image = kuva;
            this.Image.Scaling = ImageScaling.Nearest;
            peli.Add(this, 2);
        }
    }
    */
}