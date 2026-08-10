/**
 * Futuristic Dashboard - Interactive UI Enhancements
 * Adds focus states, scroll reveal animations, and micro-interactions
 */

(function () {
    'use strict';

    // ============================================
    // Search Input Focus Enhancement
    // ============================================
    function initSearchFocus() {
        const searchWrapper = document.querySelector('.fut-search-wrapper');
        const searchInput = document.querySelector('.fut-search-input');

        if (!searchWrapper || !searchInput) return;

        searchInput.addEventListener('focus', function () {
            searchWrapper.classList.add('is-focused');
        });

        searchInput.addEventListener('blur', function () {
            searchWrapper.classList.remove('is-focused');
        });
    }

    // ============================================
    // Card Scroll Reveal with Intersection Observer
    // ============================================
    function initCardReveal() {
        const cards = document.querySelectorAll('.fut-card');

        if (!cards.length) return;

        // Check if IntersectionObserver is supported
        if ('IntersectionObserver' in window) {
            const observerOptions = {
                root: null,
                rootMargin: '0px',
                threshold: 0.1
            };

            const observer = new IntersectionObserver(function (entries, observer) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                        observer.unobserve(entry.target);
                    }
                });
            }, observerOptions);

            cards.forEach(function (card) {
                observer.observe(card);
            });
        } else {
            // Fallback for browsers without IntersectionObserver
            cards.forEach(function (card) {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            });
        }
    }

    // ============================================
    // Smooth Scroll for Internal Links
    // ============================================
    function initSmoothScroll() {
        const links = document.querySelectorAll('a[href^="#"]');

        links.forEach(function (link) {
            link.addEventListener('click', function (e) {
                const targetId = this.getAttribute('href');

                if (targetId === '#') return;

                const targetElement = document.querySelector(targetId);

                if (targetElement) {
                    e.preventDefault();
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // ============================================
    // Search Form Validation Enhancement
    // ============================================
    function initSearchValidation() {
        const searchForm = document.getElementById('searchForm');
        const searchInput = document.getElementById('searchInput');
        const errorMsg = document.querySelector('.msgerror');

        if (!searchForm || !searchInput) return;

        searchForm.addEventListener('submit', function (e) {
            const query = searchInput.value.trim();

            if (query.length === 0) {
                e.preventDefault();

                if (errorMsg) {
                    errorMsg.textContent = 'Please enter a search query';
                    errorMsg.classList.remove('d-none');
                    errorMsg.classList.add('fut-error');
                }

                searchInput.focus();

                setTimeout(function () {
                    if (errorMsg) {
                        errorMsg.classList.add('d-none');
                    }
                }, 3000);

                return false;
            }

            if (errorMsg) {
                errorMsg.classList.add('d-none');
            }
        });

        // Clear error on input
        searchInput.addEventListener('input', function () {
            if (errorMsg && !errorMsg.classList.contains('d-none')) {
                errorMsg.classList.add('d-none');
            }
        });
    }

    // ============================================
    // Keyboard Shortcuts
    // ============================================
    function initKeyboardShortcuts() {
        const searchInput = document.getElementById('searchInput');

        if (!searchInput) return;

        document.addEventListener('keydown', function (e) {
            // Focus search on '/' key (like GitHub, Twitter)
            if (e.key === '/' && document.activeElement !== searchInput) {
                e.preventDefault();
                searchInput.focus();
            }

            // Clear search on Escape
            if (e.key === 'Escape' && document.activeElement === searchInput) {
                searchInput.blur();
            }
        });
    }

    // ============================================
    // Add subtle parallax effect to hero background
    // ============================================
    function initParallaxEffect() {
        const hero = document.querySelector('.fut-hero');

        if (!hero) return;

        let ticking = false;

        window.addEventListener('scroll', function () {
            if (!ticking) {
                window.requestAnimationFrame(function () {
                    const scrolled = window.pageYOffset;
                    const parallaxSpeed = 0.5;

                    if (hero && scrolled < hero.offsetHeight) {
                        hero.style.transform = 'translateY(' + (scrolled * parallaxSpeed) + 'px)';
                    }

                    ticking = false;
                });

                ticking = true;
            }
        });
    }

    // ============================================
    // Initialize All Features on DOM Ready
    // ============================================
    function init() {
        initSearchFocus();
        initCardReveal();
        initSmoothScroll();
        initSearchValidation();
        initKeyboardShortcuts();
        initParallaxEffect();
        initTheme();
        initThemeToggle();

        // Log initialization (remove in production)
        if (window.console && window.console.log) {
            console.log('Futuristic Dashboard UI initialized');
        }
    }

    // Theme utilities
    function applyTheme(theme) {
        if (theme === 'light') {
            document.documentElement.classList.add('light-theme');
            localStorage.setItem('theme', 'light');
        } else {
            document.documentElement.classList.remove('light-theme');
            localStorage.setItem('theme', 'dark');
        }
        updateThemeToggle();
    }

    function initTheme() {
        const saved = localStorage.getItem('theme');
        if (saved === 'light') {
            document.documentElement.classList.add('light-theme');
        } else {
            document.documentElement.classList.remove('light-theme');
        }
    }

    function updateThemeToggle() {
        const btn = document.getElementById('themeToggle');
        if (!btn) return;
        const isLight = document.documentElement.classList.contains('light-theme');
        btn.setAttribute('aria-pressed', isLight ? 'true' : 'false');
        btn.innerHTML = isLight ? '<i class="fas fa-sun" aria-hidden="true"></i>' : '<i class="fas fa-moon" aria-hidden="true"></i>';
    }

    function initThemeToggle() {
        const btn = document.getElementById('themeToggle');
        if (!btn) return;
        btn.addEventListener('click', function () {
            const isLight = document.documentElement.classList.contains('light-theme');
            applyTheme(isLight ? 'dark' : 'light');
        });
        updateThemeToggle();
    }

    // Run initialization
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
