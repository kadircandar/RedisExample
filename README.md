# 🚀 .NET Minimal API + Redis Integration

Bu proje, **.NET 8 Minimal API** ile **Redis cache** kullanımını örnekleyen bir yapıdır.  
Redis üzerinde **string**, **object (JSON)** ve **hash** veri tipleriyle CRUD işlemleri yapılabilmektedir. Ayrıca **pattern bazlı silme** ve **yönetimsel komutlar** (IServer kullanımı) da eklenmiştir.

---

## ✅ Özellikler
- ✔ **Redis ConnectionMultiplexer Singleton** kullanımı
- ✔ **Helper Class (RedisHelper)** ile tüm Redis işlemlerini merkezi yönetim
- ✔ **String Key-Value CRUD**
- ✔ **Object (JSON) Cacheleme**
- ✔ **Hash Veri Tipi Desteği**
- ✔ **Sorted Set (Liderlik Tablosu) Desteği**
- ✔ **Pattern bazlı silme** (`user:*` gibi)
- ✔ **Dependency Injection** uyumlu yapı
