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


## 🚀 Kurulum ve Çalıştırma Adımları

<div style="display: flex; flex-direction: column; gap: 10px;">
  <div style="background: #f8f9fa; border-radius: 10px; padding: 15px; border-left: 5px solid #28a745;">
    <strong style="color: #28a745;">1. Adım: Projeyi Klonlayın</strong><br>
    <code>git clone https://github.com/kullanici/bendensana.git</code>
  </div>
  
  <div style="background: #f8f9fa; border-radius: 10px; padding: 15px; border-left: 5px solid #007bff;">
    <strong style="color: #007bff;">2. Adım: Bağımlılıkları Yükleyin</strong><br>
    <code>dotnet restore</code>
  </div>

  <div style="background: #f8f9fa; border-radius: 10px; padding: 15px; border-left: 5px solid #6f42c1;">
    <strong style="color: #6f42c1;">3. Adım: Veritabanını Hazırlayın</strong><br>
    Visual Studio -> <i>Package Manager Console</i> ekranına şu komutu yazın:<br>
    <code>Update-Database</code>
  </div>

  <div style="background: #f8f9fa; border-radius: 10px; padding: 15px; border-left: 5px solid #dc3545;">
    <strong style="color: #dc3545;">4. Adım: Başlatın</strong><br>
    Visual Studio üzerinden <b>F5</b> tuşuna basarak projeyi ayağa kaldırın.
  </div>
</div>

---

## ⚠️ Dikkat Edilmesi Gereken Kritik Noktalar

<div style="background-color: #fff8e1; border: 1px solid #ffe082; padding: 20px; border-radius: 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.05);">
  <h3 style="color: #f57f17; margin-top: 0; display: flex; align-items: center; gap: 10px;">
    <span>🚨</span> Çalıştırmadan Önce Okuyunuz
  </h3>
  <ul style="list-style-type: none; padding-left: 0; margin-bottom: 0;">
    <li style="margin-bottom: 12px; padding-left: 25px; position: relative;">
      <span style="position: absolute; left: 0;">💾</span>
      <b>Veritabanı Şeması:</b> Projenin çalışabilmesi için <code>update-database</code> komutu ile tabloların oluşturulması <u>zorunludur</u>.
    </li>
    <li style="margin-bottom: 12px; padding-left: 25px; position: relative;">
      <span style="position: absolute; left: 0;">📦</span>
      <b>SQLite Veritabanı:</b> Veritabanı motoru olarak SQLite kullanılmıştır. <code>.db</code> dosyası ana klasörde otomatik olarak yönetilir.
    </li>
    <li style="margin-bottom: 12px; padding-left: 25px; position: relative;">
      <span style="position: absolute; left: 0;">🖼️</span>
      <b>Dinamik Resimler:</b> Görseller harici API'lerden çekildiği için her yenilemede farklı resimler gelebilir; bu bir hata değil, test verisidir.
    </li>
    <li style="margin-bottom: 12px; padding-left: 25px; position: relative;">
      <span style="position: absolute; left: 0;">🎫</span>
      <b>İndirim Kuponları:</b> Test için <code>Coupons</code> tablosundaki <code>KOD1</code> veya <code>KOD2</code> kodlarını sepet ekranında kullanabilirsiniz.
    </li>
    <li style="margin-bottom: 0; padding-left: 25px; position: relative;">
      <span style="position: absolute; left: 0;">🔄</span>
      <b>Takas Mekanizması:</b> Teklif verebilmek için kendi profilinizde yayında olan en az bir ürün bulunmalıdır.
    </li>
  </ul>
</div>

---

<div align="center" style="margin-top: 50px; padding: 20px; background: #f1f3f5; border-radius: 15px;">
  <p style="margin-bottom: 5px;"><b>👥 Proje Geliştiricileri</b></p>
  <h3 style="margin-top: 0; color: #343a40;">Ali Himeyda & Ali Efe Sarıoğlu</h3>
  <hr style="width: 50%; border: 0.5px solid #dee2e6;">
  <p style="font-style: italic; color: #6c757d;">Bu proje, Sakarya Üniversitesi Bilgisayar Mühendisliği bölümü kapsamında geliştirilmiş bir bitirme ödevi çalışmasıdır.</p>
</div>
