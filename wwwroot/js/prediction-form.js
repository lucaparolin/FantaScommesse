// FantaScommesse - Prediction Form Handler
// Interactive prediction submission with real-time validation

class PredictionForm {
    constructor() {
        this.selections = {};
        this.counters = { doubles: 0, singles: 0, specials: 0 };
        this.maxCounts = { doubles: 4, singles: 3, specials: 3 };
        this.init();
    }

    init() {
        this.loadMatches();
        this.attachEventListeners();
        this.loadDraft();
    }

    async loadMatches() {
        // Get round ID from URL or data attribute
        const roundId = document.querySelector('#prediction-form').dataset.roundId || 1;

        try {
            const response = await fetch(`/api/v1/rounds/${roundId}`, {
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem('token')
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.renderMatches(data.matches);
            }
        } catch (error) {
            console.error('Error loading matches:', error);
            this.showToast('Errore nel caricamento delle partite', 'error');
        }
    }

    renderMatches(matches) {
        matches.forEach((match, index) => {
            const matchTeamsEl = document.getElementById(`match-${index + 1}-teams`);
            if (matchTeamsEl) {
                matchTeamsEl.textContent = `${match.homeTeam} vs ${match.awayTeam}`;
            }
        });
    }

    attachEventListeners() {
        // Selection buttons
        document.querySelectorAll('.btn-selection').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const matchNo = e.target.dataset.match;
                const type = e.target.dataset.type;
                const value = e.target.dataset.value;

                this.handleSelection(matchNo, type, value, e.target);
            });
        });

        // Save draft button
        document.getElementById('save-draft')?.addEventListener('click', () => {
            this.saveDraft();
        });

        // Submit button
        document.getElementById('submit-prediction')?.addEventListener('click', (e) => {
            e.preventDefault();
            this.submitPrediction();
        });
    }

    handleSelection(matchNo, type, value, buttonEl) {
        const previousSelection = this.selections[matchNo];

        // Remove previous selection for this match
        if (previousSelection) {
            document.querySelectorAll(`[data-match="${matchNo}"].selected`).forEach(el => {
                el.classList.remove('selected');
            });

            // Decrement previous counter
            const prevType = previousSelection.type;
            this.counters[prevType + 's']--;
        }

        // Check if we can add this selection type
        if (this.counters[type + 's'] >= this.maxCounts[type + 's']) {
            this.showToast(`Hai già raggiunto il massimo di ${this.maxCounts[type + 's']} ${this.getTypeLabel(type)}`, 'warning');
            return;
        }

        // Add new selection
        this.selections[matchNo] = { type, value };
        this.counters[type + 's']++;
        buttonEl.classList.add('selected');

        this.updateCounters();
        this.autoSaveDraft();
    }

    updateCounters() {
        document.getElementById('doubles-count').textContent = this.counters.doubles;
        document.getElementById('singles-count').textContent = this.counters.singles;
        document.getElementById('specials-count').textContent = this.counters.specials;

        // Highlight counters if complete or exceeded
        this.highlightCounter('doubles');
        this.highlightCounter('singles');
        this.highlightCounter('specials');
    }

    highlightCounter(type) {
        const counterEl = document.querySelector(`.counter:has(#${type}-count)`);
        if (!counterEl) return;

        const count = this.counters[type];
        const max = this.maxCounts[type];

        counterEl.style.borderLeft = count === max ? '4px solid var(--success)' : count > max ? '4px solid var(--danger)' : 'none';
    }

    validate() {
        const errors = [];

        // Check total selections
        const totalSelections = Object.keys(this.selections).length;
        if (totalSelections !== 10) {
            errors.push(`Devi completare tutti i 10 pronostici. Attualmente: ${totalSelections}/10`);
        }

        // Check counts
        if (this.counters.doubles !== 4) {
            errors.push(`Servono esattamente 4 doppie (hai: ${this.counters.doubles})`);
        }

        if (this.counters.singles !== 3) {
            errors.push(`Servono esattamente 3 fisse (hai: ${this.counters.singles})`);
        }

        if (this.counters.specials !== 3) {
            errors.push(`Servono esattamente 3 speciali (hai: ${this.counters.specials})`);
        }

        return errors;
    }

    async saveDraft() {
        const roundId = document.querySelector('#prediction-form').dataset.roundId || 1;

        const data = {
            roundId: parseInt(roundId),
            isDraft: true,
            items: Object.entries(this.selections).map(([matchNo, sel]) => ({
                matchId: parseInt(matchNo),
                selection: sel.value
            }))
        };

        try {
            const response = await fetch(`/api/v1/predictions/${roundId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + localStorage.getItem('token')
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                this.showToast('Bozza salvata con successo', 'success');
                localStorage.setItem(`draft_${roundId}`, JSON.stringify(this.selections));
            } else {
                const error = await response.json();
                this.showToast(error.message || 'Errore nel salvataggio', 'error');
            }
        } catch (error) {
            console.error('Error saving draft:', error);
            this.showToast('Errore di connessione', 'error');
        }
    }

    async submitPrediction() {
        // Validate first
        const errors = this.validate();

        if (errors.length > 0) {
            this.showToast(errors.join('<br>'), 'error');
            return;
        }

        // Confirm submission
        if (!confirm('Sei sicuro di voler inviare il pronostico? Non potrai più modificarlo!')) {
            return;
        }

        const roundId = document.querySelector('#prediction-form').dataset.roundId || 1;

        const data = {
            roundId: parseInt(roundId),
            isDraft: false,
            items: Object.entries(this.selections).map(([matchNo, sel]) => ({
                matchId: parseInt(matchNo),
                selection: sel.value
            }))
        };

        try {
            const response = await fetch(`/api/v1/predictions/${roundId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + localStorage.getItem('token')
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                const result = await response.json();
                this.showToast('✅ Pronostico inviato con successo!', 'success');

                // Clear draft
                localStorage.removeItem(`draft_${roundId}`);

                // Redirect after 2 seconds
                setTimeout(() => {
                    window.location.href = '/';
                }, 2000);
            } else {
                const error = await response.json();
                this.showToast(error.message || 'Errore nell\'invio', 'error');
            }
        } catch (error) {
            console.error('Error submitting prediction:', error);
            this.showToast('Errore di connessione', 'error');
        }
    }

    autoSaveDraft() {
        // Auto-save draft every 30 seconds
        clearTimeout(this.autoSaveTimeout);
        this.autoSaveTimeout = setTimeout(() => {
            if (Object.keys(this.selections).length > 0) {
                const roundId = document.querySelector('#prediction-form').dataset.roundId || 1;
                localStorage.setItem(`draft_${roundId}`, JSON.stringify(this.selections));
                console.log('Draft auto-saved');
            }
        }, 30000);
    }

    loadDraft() {
        const roundId = document.querySelector('#prediction-form').dataset.roundId || 1;
        const savedDraft = localStorage.getItem(`draft_${roundId}`);

        if (savedDraft) {
            try {
                const draft = JSON.parse(savedDraft);

                Object.entries(draft).forEach(([matchNo, selection]) => {
                    const button = document.querySelector(`[data-match="${matchNo}"][data-value="${selection.value}"]`);
                    if (button) {
                        button.click();
                    }
                });

                this.showToast('Bozza precedente ripristinata', 'success');
            } catch (error) {
                console.error('Error loading draft:', error);
            }
        }
    }

    showToast(message, type = 'info') {
        // Remove existing toast
        document.querySelectorAll('.toast').forEach(t => t.remove());

        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = message;
        document.body.appendChild(toast);

        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 5000);
    }

    getTypeLabel(type) {
        const labels = {
            double: 'doppie',
            single: 'fisse',
            special: 'speciali'
        };
        return labels[type] || type;
    }
}

// Initialize when DOM is ready
if (document.getElementById('prediction-form')) {
    document.addEventListener('DOMContentLoaded', () => {
        new PredictionForm();
    });
}
