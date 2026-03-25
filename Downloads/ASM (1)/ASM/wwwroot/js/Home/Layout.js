document.addEventListener("DOMContentLoaded", function () {
    const selectHang = document.querySelector(".chonHang");
    const inputSearch = document.querySelector(".khungTimKiem input[type=text]");
    const btnSearch = document.querySelector(".iconTimKiem");

    if (!selectHang || !inputSearch || !btnSearch) return;

    function xuLyTimKiem() {
        const hang = selectHang.value.trim();     //Mỳ cay
        const tuKhoa = inputSearch.value.trim();

        let url = "";

        if (hang && tuKhoa) {
            // Tìm theo hãng + từ khóa
            url = `/SanPham?brand=${hang}&q=${encodeURIComponent(tuKhoa)}`;
        }
        else if (hang && !tuKhoa) {
            // Chỉ chọn hãng → /SanPham/Flydigi
            const tenTrang = hang.charAt(0).toUpperCase() + hang.slice(1);
            url = `/SanPham/${tenTrang}`;
        }
        else if (!hang && tuKhoa) {
            // Chỉ tìm từ khóa
            url = `/SanPham?q=${encodeURIComponent(tuKhoa)}`;
        }
        else {
            // Không nhập gì
            url = "/SanPham";
        }

        window.location.href = url;
    }

    btnSearch.addEventListener("click", xuLyTimKiem);
    inputSearch.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
            xuLyTimKiem();
        }
    });

    

});