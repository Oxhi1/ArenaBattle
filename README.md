# 2D Arena Savaş Oyunu

Küçük bir 2D arena içinde oyuncu ve rakibin çeşitli aksiyonlar gerçekleştirdiği, temel oynanış mekaniği ve menü sistemine sahip Unity projesi.

## Oynanış (Web Build)

Oyunun itch.io veya simmer.io bağlantısını buraya ekleyeceksin:
https://...

## Proje Reposu

Bu repo Unity projesinin tüm dosyalarını içerir.  


## Projenin Özeti

2D arena şeklinde küçük bir savaş sahası.

### Oyuncu
- Hareket eder
- Mermi ateşler
- Dash yapar
- Kendini iyileştirir  


### Rakip
- Hasar alır
- Stun olur
- Roar animasyonu oynatır
- Shield animasyonu oynatır  


Oyuncunun aksiyonları rakibi; rakibin aksiyonları oyuncuyu etkileyebilir niteliktedir.

### Ana Menü
- Yeni oyun başlat
- Müzik ses ayarı
- Efekt ses ayarı

## Kurallara Uygunluk Kontrol Listesi

| Kural                              | Durum | Açıklama                                           |
|------------------------------------|:-----:|----------------------------------------------------|
| Oyuncu + Rakip karakter            |  ✔️   | Player ve Enemy prefabları mevcut                  |
| Player aksiyon                     |  ✔️   | Hareket, Ateş, Dash, Heal                          |
| Enemy  aksiyon                     |  ✔️   | Damage alma, Stun, Roar, Shield                    |
| Aksiyonların karşılıklı etkisi     |  ✔️   | Mermiler sadece rakibi etkiler; enemy hasar verir  |
| Fizik dışı aksiyon                 |  ✔️   | Heal / Dash / Shoot farklı etkiler                 |
| Ana menü + müzik & ses ayarı       |  ✔️   | Slider + butonlar hazır                            |
| Web Build & bağlantı               |  ⬜   | (itch.io linki eklenecek)                          |

## Oynanış Kontrolleri

| Eylem       | Tuş       |
|-------------|-----------|
| Hareket     | WASD      |
| Ateş        | Sol Mouse |
| Dash        | Space     |
| Heal        | Q         |
| Pause Menü  | ESC       |

## Teknik Notlar

- Unity ile geliştirilmiştir.
- Player ve Enemy prefabları sahnede ayrıştırılmıştır.
- Rakip davranışları AI içermez; yalnızca tetiklenebilir animasyon ve durumlar mevcuttur.
- Ana menüde ses ayarları slider ile kontrol edilir (Müzik ve Efekt).


## Lisans

Bu proje eğitim amaçlıdır. Gerekli ise lisans bilgisi burada belirtilebilir.
