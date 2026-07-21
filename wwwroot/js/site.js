document.addEventListener('DOMContentLoaded', function () {

    // ===================================================
    // 1. ULTIMATE SAFE SCROLL-REVEAL FORCING VIEW
    // ===================================================
    const revealEls = document.querySelectorAll('.fade-in-up');

    // பக்கத்தில் இருக்கும் அனிமேஷன் எலிமெண்ட்டுகளை உடனடியாகக் காண்பிக்கச் செய்யும் Force Logic
    if (revealEls.length > 0) {
        revealEls.forEach(el => {
            // CSS-ல் ஏதேனும் மறைத்து வைக்கப்பட்டிருந்தால் அதை உடைக்க நேரடி ஸ்டைல் மாற்றி அமைக்கப்படுகிறது
            el.style.opacity = "1";
            el.style.transform = "none";
            el.style.visibility = "visible";
            el.classList.add('is-visible');
        });
    }

    // ===================================================
    // 2. STICKY NAV BACKGROUND CHANGE ON SCROLL
    // ===================================================
    const nav = document.querySelector('.site-nav-premium');
    if (nav) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 40) {
                nav.classList.add('nav-scrolled');
            } else {
                nav.classList.remove('nav-scrolled');
            }
        });
    }

    // ===================================================
    // 3. PREMIUM INTERACTIVE SLIDER LANGUAGE TOGGLE LOGIC
    // ===================================================
    const langContainer = document.getElementById('langContainer');
    const slider = document.getElementById("langSlider");
    const optEN = document.getElementById("optEN");
    const optTN = document.getElementById("optTN");

    // LocalStorage-ல் இருந்து ஏற்கனவே சேமிக்கப்பட்ட மொழியைச் சரிபார்க்கும்
    const savedLang = localStorage.getItem("brtLanguage") || "EN";
    if (savedLang === "TN") {
        document.body.classList.add('lang-ta');
        setSliderUI("TN");
    } else {
        document.body.classList.remove('lang-ta');
        setSliderUI("EN");
    }

    if (langContainer) {
        langContainer.addEventListener('click', function () {
            // Body-ல் lang-ta கிளாஸை மாற்றி அமைக்கும் (Toggle)
            const isTamil = document.body.classList.toggle('lang-ta');

            if (isTamil) {
                localStorage.setItem("brtLanguage", "TN");
                setSliderUI("TN");
            } else {
                localStorage.setItem("brtLanguage", "EN");
                setSliderUI("EN");
            }
        });
    }

    function setSliderUI(lang) {
        if (!slider || !optEN || !optTN) return;

        if (lang === "EN") {
            slider.style.left = "4px";
            slider.style.width = "44px";
            optEN.classList.add("active");
            optTN.classList.remove("active");
        } else {
            slider.style.left = "44px";
            slider.style.width = "62px";
            optTN.classList.add("active");
            optEN.classList.remove("active");
        }
    }

    // ===================================================
    // 4. ANIMATED STATISTICS COUNTER
    // ===================================================
    const counters = document.querySelectorAll('.counter');
    if (counters.length > 0) {
        const counterObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const el = entry.target;
                    const target = parseInt(el.getAttribute('data-target'), 10) || 0;
                    const duration = 1400;
                    const start = performance.now();

                    function tick(now) {
                        const progress = Math.min((now - start) / duration, 1);
                        const eased = 1 - Math.pow(1 - progress, 3);
                        el.textContent = Math.floor(eased * target) + (progress >= 1 ? '+' : '');
                        if (progress < 1) requestAnimationFrame(tick);
                        else el.textContent = target + '+';
                    }
                    requestAnimationFrame(tick);
                    counterObserver.unobserve(el);
                }
            });
        }, { threshold: 0.4 });
        counters.forEach(c => counterObserver.observe(c));
    }
});