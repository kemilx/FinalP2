(() => {
    document.addEventListener('DOMContentLoaded', () => {
        const filterButtons = document.querySelectorAll('[data-filter]');
        const moduleCards = document.querySelectorAll('[data-module-card]');
        filterButtons.forEach((button) => {
            button.addEventListener('click', () => {
                filterButtons.forEach((btn) => btn.classList.remove('active'));
                button.classList.add('active');
                const target = button.getAttribute('data-filter');
                moduleCards.forEach((card) => {
                    const category = card.getAttribute('data-category');
                    if (!target || target === 'all' || category === target) {
                        card.classList.remove('d-none');
                    } else {
                        card.classList.add('d-none');
                    }
                });
            });
        });

        const capacityRange = document.getElementById('capacityRange');
        const capacityValue = document.getElementById('capacityValue');
        const capacityProgress = document.getElementById('capacityProgress');
        if (capacityRange && capacityValue && capacityProgress) {
            const updateCapacity = () => {
                const value = capacityRange.value;
                capacityValue.textContent = `${value}%`;
                capacityProgress.style.width = `${value}%`;
            };
            capacityRange.addEventListener('input', updateCapacity);
            updateCapacity();
        }

        const activityItems = document.querySelectorAll('[data-activity-item]');
        const detailTitle = document.getElementById('activityDetailTitle');
        const detailText = document.getElementById('activityDetailText');
        const detailTime = document.getElementById('activityDetailTime');
        activityItems.forEach((item) => {
            item.addEventListener('click', () => {
                activityItems.forEach((i) => i.classList.remove('active'));
                item.classList.add('active');
                if (detailTitle) {
                    detailTitle.textContent = item.getAttribute('data-activity-title') ?? '';
                }
                if (detailText) {
                    detailText.textContent = item.getAttribute('data-activity-details') ?? '';
                }
                if (detailTime) {
                    detailTime.textContent = item.getAttribute('data-activity-time') ?? '';
                }
            });
        });
    });
})();
