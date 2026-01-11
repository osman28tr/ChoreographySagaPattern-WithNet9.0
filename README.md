<h1>Choreography Design Pattern ile Mikroservislerde Transaction Yönetimi</h1>

<h1>1- Giriş</h1>
<p align="justify">Projede order, stock ve fakepayment mikroservisleri oluşturularak mikroservislerde transaction yönetimi, bununla beraber veri tutarlılığını asenkron bi şekilde sağlanmış olup, temiz bir mimari ve kodlama ile gerçekleştirilmiştir. </p>

<h3>Saga Pattern ve Choreography Tabanlı Saga Pattern Nedir?</h3>
<p align="justify">Saga desig pattern, Mikroservisler arası veri tutarlılığını sağlamak için kullanılır. Birden fazla mikroservis arasında transaction yönetiminde kullanılır. Eğer tek bir veritabanı ile çalışılıyorsa burada kullanılan framework transaction yönetimini otomatik olarak yapmaktadır. Saga’nın iki tane implementasyonu vardır</p>

<p align="justify"><b>1-Choreography Based Saga </b></p>

<ul>

<li>Local transaction sırasını kullanarak bir transaction yönetimi sağlar. (yani her bir mikroservis kendi scope’u içerisindeki işlemleri gerçekleştirir. Başka mikroservislerin transactionlarını bilmez.)</li>
<li>2-4 mikroservis arasında bir distributed transaction yönetimi için uygun bir implementasyondur.</li>
<li>Sisteme katılan her bir katılımcı karar vericidir. (başarılı/başarısız)</li>
<li>İmplemente etmenin bir yolu asenkron messaging pattern kullanmaktır.</li>
<li>Her bir servis kuyruğu dinler, gelen event/message ile ilgili işlemi yapar, sonuç olarak başarılı veya başarısız durumunu tekrar kuyruğa döner</li>
<li>Point to point bir iletişim olmadığı için servisler arası coupling azalır.</li>
<li>Transaction yönetimi merkezi olmadığı için performans darboğazı azalır.</li>

</ul>