<div align="center">
  <img src="https://placehold.co/600x200/DB4444/white?text=BendenSana+E-Ticaret+%26+Takas" alt="Logo" width="100%">

  # 🛍️ BendenSana | Modern E-Ticaret ve Takas Platformu
  
  <p align="center">
    <strong>Gelişmiş Takas Algoritmaları | Rol Bazlı Yönetim | Dinamik İstatistikler</strong>
  </p>

  ---
</div>

## 📖 Proje Hakkında
**BendenSana**, kullanıcıların standart bir alışveriş deneyiminin ötesine geçerek sahip oldukları ürünleri takas edebildikleri hibrit bir e-ticaret platformudur. Proje, ölçeklenebilir bir mimari üzerine inşa edilmiş olup yüksek performans ve kullanıcı deneyimi odaklı geliştirilmiştir.

### ✨ Temel Özellikler
<table>
  <tr>
    <td><b>🔄 Takas Sistemi</b></td>
    <td>Ürün karşılığı ürün + nakit teklifleri sunabilme ve yönetebilme.</td>
  </tr>
  <tr>
    <td><b>📊 Analiz Paneli</b></td>
    <td>Satışların, siparişlerin ve ziyaretçi verilerinin Chart.js ile görselleştirilmesi.</td>
  </tr>
  <tr>
    <td><b>🛡️ Rol Yönetimi</b></td>
    <td>Admin, Satıcı ve Alıcı rollerine özel yetkilendirilmiş paneller.</td>
  </tr>
  <tr>
    <td><b>🔍 Akıllı Filtreleme</b></td>
    <td>Kategori, renk, fiyat ve cinsiyet bazlı anlık daraltma motoru.</td>
  </tr>
</table>

---

## 🛠️ Teknik Altyapı ve Mimari
Proje, kurumsal standartlarda **Clean Architecture** prensiplerine uygun olarak tasarlanmıştır.

- **Framework:** `.NET 8.0 ASP.NET Core MVC`
- **Veritabanı:** `SQLite` (Hızlı kurulum ve taşınabilirlik için)
- **ORM:** `Entity Framework Core` (Code First Yaklaşımı)
- **Kimlik Yönetimi:** `ASP.NET Core Identity`
- **Tasarım Deseni:** `Repository Pattern` (Veri erişim katmanı soyutlaması)
- **Frontend:** `Bootstrap 5`, `Chart.js`, `FontAwesome`

---

## 📁 Klasör Yapısı
```text
BendenSana/
├── 🎮 Controllers/    # Business Logic ve HTTP istek yönetimi
├── 📦 Models/         # Veritabanı tabloları ve Entity tanımları
├── 📋 ViewModels/     # Sayfa bazlı veri transfer nesneleri (DTO)
├── 🏗️ Repositories/   # IRepository arayüzleri ve somut sınıflar
├── 🖼️ wwwroot/        # CSS, JS, Resimler ve statik içerikler
└── 🍱 Views/          # Razor View (HTML) dosyaları

🚀 Kurulum Adımları
Projeyi yerel ortamınızda ayağa kaldırmak için aşağıdaki adımları izleyin:

Repoyu Klonlayın: git clone https://github.com/kullanici/bendensana.git

Paketleri Geri Yükleyin: dotnet restore

Veritabanını Oluşturun: Visual Studio içindeki Package Manager Console üzerinden Update-Database komutunu çalıştırın.

Çalıştırın: F5 tuşuna basarak uygulamayı başlatın.

<div style="background-color: #fff3cd; border-left: 6px solid #ffecb5; padding: 15px; border-radius: 8px;"> <h3>⚠️ Proje Çalıştırılmadan Önce Dikkat Edilmesi Gerekenler</h3> <ul> <li><b>Veritabanı Şeması:</b> Proje çalışmadan önce <code>update-database</code> komutu mutlaka çalıştırılmalıdır.</li> <li><b>SQLite Kullanımı:</b> Veritabanı olarak SQLite tercih edilmiştir. Veritabanı dosyası ana dizinde <code>.db</code> uzantılı olarak otomatik oluşturulur.</li> <li><b>Dinamik Resimler:</b> Proje genelindeki görseller harici API'ler (Picsum vb.) üzerinden çekilmektedir. Her sayfa yenilemesinde görseller değişkenlik gösterebilir.</li> <li><b>İndirim Kuponları:</b> Sepet tutarına indirim uygulamak için <code>Coupons</code> tablosunda tanımlı kodlar (Örn: <code>KOD1</code>, <code>KOD2</code>) kullanılabilir.</li> <li><b>Takas Şartı:</b> Takas teklifi sunabilmek için sisteme en az bir adet ürün kaydetmiş olmanız gerekmektedir.</li> </ul> </div>

<div align="center"> <p><b>Geliştirici:</b> Ali Himeyda , Ali Efe Sarıoğlu</p> <p><i>Bu proje eğitim amaçlı geliştirilmiş bir bitirme ödevi çalışmasıdır.</i></p> </div>


**Next Step:** Projeniz için bir **Database Script** oluşturmak veya **YouTube Sunum Vi
