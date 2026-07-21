document.addEventListener('DOMContentLoaded', function () {
    // Scroll-reveal animation for elements marked .fade-in-up
    const revealEls = document.querySelectorAll('.fade-in-up');
    revealEls.forEach(el => el.classList.add('fade-prep'));

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    revealEls.forEach(el => observer.observe(el));

    setTimeout(() => {
        document.querySelectorAll('.fade-prep:not(.is-visible)').forEach(el => el.classList.add('is-visible'));
    }, 3000);

    // Shrink sticky nav on scroll
    const nav = document.querySelector('.site-nav');
    if (nav) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 40) nav.classList.add('nav-scrolled');
            else nav.classList.remove('nav-scrolled');
        });
    }

    // ===================================================
    // LANGUAGE TOGGLE SYSTEM (langContainer / optEN / optTN / langSlider)
    // Persists selection across page navigation using localStorage
    // ===================================================
    const langContainer = document.getElementById('langContainer');
    const langSlider = document.getElementById('langSlider');
    const optEN = document.getElementById('optEN');
    const optTN = document.getElementById('optTN');

    function applyLanguage(lang) {
        // Set the lang attribute on <html> - this is what all the CSS rules
        // (html[lang="en"] .lang-en, html[lang="ta"] .lang-ta, etc.) key off of
        document.documentElement.setAttribute('lang', lang);
        document.body.setAttribute('lang', lang);

        if (optEN && optTN) {
            optEN.classList.toggle('active', lang === 'en');
            optTN.classList.toggle('active', lang === 'ta');
        }

        if (langSlider && langContainer) {
            if (lang === 'ta' && optTN) {
                langSlider.style.left = optTN.offsetLeft + 'px';
                langSlider.style.width = optTN.offsetWidth + 'px';
            } else if (optEN) {
                langSlider.style.left = optEN.offsetLeft + 'px';
                langSlider.style.width = optEN.offsetWidth + 'px';
            }
        }

        localStorage.setItem('brt-lang', lang);
    }

    if (langContainer) {
        // Restore saved language on page load (default: English)
        const savedLang = localStorage.getItem('brt-lang') || 'en';
        applyLanguage(savedLang);

        langContainer.addEventListener('click', function () {
            const currentLang = document.documentElement.getAttribute('lang') === 'ta' ? 'ta' : 'en';
            const newLang = currentLang === 'en' ? 'ta' : 'en';
            applyLanguage(newLang);
        });
    }

    // Animated statistics counter
    const counters = document.querySelectorAll('.counter');
    if (counters.length) {
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
