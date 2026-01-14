# 2D Arena Savaş Oyunu

Küçük bir 2D arena içinde oyuncu ve rakibin çeşitli aksiyonlar gerçekleştirdiği, temel oynanış mekaniği ve menü sistemine sahip Unity projesi.

## Oynanış (Web Build)

[(https://abdullahcelik.itch.io/arena-mini-battle)]

## Proje Reposu

Bu repo Unity projesinin tüm dosyalarını içerir.  


## Projenin Özeti

2D arena şeklinde küçük bir savaş sahası.

### Oyuncu
- Hareket eder
- Mermi ateşler
- Alan etkili saldırı yapar.
- Zehir saldırısı yapar(rakibi yavaşlatır/hasar verir)
- Canı azalır/artar
- Normal saldırı yapabilir


### Rakip (ai kontrolünde)
- Hareket eder
- Ağır vuruş yapabilir
- Laser ateşleyebilir
- Dash atabilir
- Normal vuruş atabilir
- Canı azalır/Artar
- Oyuncuya göre hareket edebilir  


Oyuncunun aksiyonları rakibi; rakibin aksiyonları oyuncuyu etkileyebilir niteliktedir.

### Ana Menü
- Yeni oyun başlat
- Müzik ses ayarı
- Efekt ses ayarı

## Kurallara Uygunluk Kontrol Listesi

| Kural                              | Durum | Açıklama                                           |
|------------------------------------|:-----:|----------------------------------------------------|
| Oyuncu + Rakip karakter            |  ✔️   | Player ve Enemy prefabları mevcut                  |
| Player aksiyon                     |  ✔️   | Hareket, Ateş, shockwave, poison                   |
| Enemy  aksiyon                     |  ✔️   | Hareket, heavy, laser, dash,punch                  |
| Aksiyonların karşılıklı etkisi     |  ✔️   | Mermiler sadece rakibi etkiler; enemy hasar verir  |
| Ana menü + müzik & ses ayarı       |  ✔️   | Slider + butonlar hazır                            |


## Oynanış Kontrolleri

| Eylem       | Tuş       |
|-------------|-----------|
| Hareket     | WASD      |
| Punch        | Sol Mouse ||Oyuncu basic saldırısı(yönlendirilebilir)|
| Mermi saldırsı        | Sağ mouse | |Uzak mesafe saldırısı(yönlendirilebilir)|
| Poison saldırısı        | Q ||orta mesafe saldırısı düşmana hasar verir ve yavaşlatır(direkt saldırı)|
|Shockwave saldırısı | E||Oyuncunun tüm çevresine anlık olarak gerçekleşir(alan etkili)|
| Pause Menü  | ESC |

## Teknik Notlar

- Unity ile geliştirilmiştir.
- Player ve Enemy prefabları sahnede ayrıştırılmıştır.
- Rakip davranışları Q-Learning ile geliştirilmiştir.
- Ana menüde ses ayarları slider ile kontrol edilir (Müzik ve Efekt).



