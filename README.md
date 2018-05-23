# TrafficSignRecognition
TrafficSignRecognition

Merhaba,

Bitirme Projesi olarak yaptığım bu uygulamada;
Hazır olarak, daha önceden çekilmiş/kaydedilmiş bir elektronik ortamdaki görüntüden,
'Canny' kenar tespit algoritması ile trafik işaretinin kenarları tespit edilip devamında 'Freeman Chain Code' metodu ile
görüntünün kendine özgün bir imzası niteliğinde veri elde edilir.
Bu veri daha sonra veritabanındaki verilerle karşılaştırılıp hangi trafik işaretinin imzasına daha çok benziyorsa, 
sonuç o trafik işareti olarak döndürülür.
Bitirme projemizin süresi 3 ay gibi kısa bir süre olduğundan şimdilik hazır görüntü kullandım.
İlerleyen zamanlarda ise geliştirip anlık görüntüden tespit işlemi gerçekleştirmeyi planlıyorum.

*************************************************************************************************************************
Uygulamada 'OpenCV' kütüphanesinin C#' uyarlaması(wrapper) olan 'EmguCV' kullanılmıştır.
Bunun için öncelikle bilgisayara 'EmguCv' kurulup Visual Studio'ya eklenmelidir.
*************************************************************************************************************************
