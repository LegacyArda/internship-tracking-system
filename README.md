# 🎓 Staj Takip ve Değerlendirme Sistemi

Staj sürecini **öğrenci, akademisyen ve yönetici** rolleriyle tek yerden yürüten ASP.NET Core MVC web uygulaması. Mezuniyet projesi olarak geliştirildi.

<!-- Ekran görüntüsü ekle (gerçek kişi verisi olmasın): docs/ klasörüne koy, aşağıdaki satırları aç.
![Ana ekran](docs/dashboard.png)
![Staj raporu](docs/rapor.png)
-->

## Özellikler
- **Rol tabanlı yetkilendirme:** Öğrenci, Akademisyen, Yönetici (ASP.NET Core Identity + yetki politikaları)
- **Staj raporu yönetimi:** Rapor oluşturma, düzenleme, silme, detay görüntüleme
- **Güvenli dosya yükleme:** Rapora belge ekleme
- **Geri bildirim ve puanlama:** Akademisyen değerlendirmesi ve puan sistemi
- **Dinamik PDF rapor çıktısı:** QuestPDF ile
- **Filtrelenebilir yönetim paneli**

## Teknolojiler
| Katman | Kullanılan |
|---|---|
| Backend | C#, ASP.NET Core MVC (.NET 10), Razor Pages (Identity) |
| Veri | Entity Framework Core, SQL Server (LocalDB) |
| Kimlik | ASP.NET Core Identity, rol ve politika tabanlı yetkilendirme |
| PDF | QuestPDF |
| Arayüz | HTML5, CSS3, JavaScript, Bootstrap |

## Kurulum
**Gereksinimler:** .NET 10 SDK, SQL Server LocalDB (Visual Studio ile gelir), `dotnet-ef` aracı (`dotnet tool install --global dotnet-ef`)

```bash
git clone https://github.com/LegacyArda/internship-tracking-system.git
cd internship-tracking-system/StajTakipSistemi

# Yönetici hesabı bilgilerini gizli yapılandırmaya ekle (repo'ya yazılmaz)
dotnet user-secrets init
dotnet user-secrets set "AdminSettings:AdminEmail" "admin@ornek.com"
dotnet user-secrets set "AdminSettings:AdminPassword" "<kendi-güçlü-şifren>"

# Veritabanını oluştur (bağlantı: appsettings.json > DefaultConnection, varsayılan LocalDB / StajTakipDB)
dotnet ef database update

dotnet run
```
Uygulama açılışında `Öğrenci`, `Akademisyen` ve `Yönetici` rolleri otomatik oluşturulur; belirttiğin yönetici hesabı `Yönetici` rolüne atanır. Öğrenci ve akademisyen hesaplarını kayıt sayfasından oluşturabilirsin.

## Proje yapısı
```
StajTakipSistemi/
├── Controllers/   # InternshipReports, ogrencis, Home, profile
├── Models/        # InternshipReport, ogrenci
├── Data/          # ApplicationDbContext
├── Filters/       # AdminGlobalFilter
├── Migrations/    # EF Core migration geçmişi
├── Views/         # Razor görünümleri
└── Areas/Identity # Giriş / kayıt sayfaları
```

## Lisans
MIT
