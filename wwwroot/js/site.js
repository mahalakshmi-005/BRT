document.addEventListener('DOMContentLoaded', function () {
    // Scroll-reveal animation for elements marked .fade-in-up
    // Safe-by-default: only add the "hidden until revealed" state once JS is confirmed running.
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

    // Safety net: if anything is still hidden after 3s (observer edge cases, layout quirks), reveal it anyway
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

    // English / Tamil display toggle (delegated — works even if button is re-rendered)
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('#langToggle');
        if (!btn) return;
        document.body.classList.toggle('lang-ta');
        btn.textContent = document.body.classList.contains('lang-ta') ? 'தமிழ் | EN' : 'EN | தமிழ்';
    });

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
