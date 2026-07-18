class CyberChefClient {
    constructor() {
        this.operations = [];
        this.recipe = [];
        this.baseUrl = '/api';
        this.autoBakeTimeout = null;
        this.isExecuting = false;
        this.autoBakeEnabled = true;
        this.init();
    }

    async init() {
        console.log('🚀 Initializing...');
        await this.loadOperations();
        this.setupThemeSelector();
        this.setupEventListeners();
        this.renderOperations();
        this.loadRecipeFromStorage();
        this.renderRecipe();
        this.updateAutoBakeUI();
        this.showLog('🚀 CyberChef Clone ready!', 'success');

        setTimeout(() => {
            this.triggerAutoBake();
        }, 500);
    }

    async loadOperations() {
        try {
            console.log('📥 Loading operations...');
            const response = await fetch(`${this.baseUrl}/operations`);
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }
            this.operations = await response.json();
            console.log(`✅ Loaded ${this.operations.length} operations`);
        } catch (error) {
            console.error('❌ Failed to load operations:', error);
            this.showError('Failed to load operations: ' + error.message);
        }
    }

    setupThemeSelector() {
        const themeSelect = document.getElementById('themeSelect');
        if (!themeSelect) return;

        const savedTheme = localStorage.getItem('cyberchef-theme') || 'default';
        themeSelect.value = savedTheme;
        this.applyTheme(savedTheme);

        themeSelect.addEventListener('change', (e) => {
            const theme = e.target.value;
            this.applyTheme(theme);
            localStorage.setItem('cyberchef-theme', theme);
            this.showLog(`🎨 Theme changed to: ${theme}`, 'info');
        });
    }

    applyTheme(theme) {
        const body = document.body;
        const classes = body.className.split(' ').filter(c => !c.startsWith('theme-'));
        body.className = classes.join(' ');
        if (theme && theme !== 'default') {
            body.classList.add(`theme-${theme}`);
        }
    }

    setupEventListeners() {
        console.log('🔗 Setting up event listeners...');

        // Search operations
        document.getElementById('searchOperations').addEventListener('input', (e) => {
            this.renderOperations(e.target.value);
        });

        // Add operation
        document.getElementById('operationsList').addEventListener('click', (e) => {
            const item = e.target.closest('.operation-item');
            if (item) {
                const opName = item.dataset.operation;
                const operation = this.operations.find(op => op.name === opName);
                if (operation) {
                    this.addToRecipe(operation);
                }
            }
        });

        // Remove step
        document.getElementById('recipeList').addEventListener('click', (e) => {
            if (e.target.classList.contains('remove-step')) {
                const index = parseInt(e.target.dataset.index);
                if (!isNaN(index)) {
                    this.removeFromRecipe(index);
                }
            }
        });

        // Clear recipe
        document.getElementById('clearRecipeBtn').addEventListener('click', () => {
            this.recipe = [];
            this.renderRecipe();
            this.clearRecipeFromStorage();
            this.clearLog();
            document.getElementById('inputData').value = '';
            document.getElementById('outputData').value = '';
            this.showLog('🗑️ Recipe cleared', 'info');
            this.triggerAutoBake();
        });

        // Load file
        document.getElementById('loadFileBtn').addEventListener('click', () => {
            document.getElementById('fileInput').click();
        });

        document.getElementById('fileInput').addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (event) => {
                    document.getElementById('inputData').value = event.target.result;
                    this.showLog('📁 Loaded file: ' + file.name + ' (' + (file.size / 1024).toFixed(1) + ' KB)', 'success');
                    this.saveRecipeToStorage();
                    this.triggerAutoBake();
                };
                reader.onerror = () => {
                    this.showLog('❌ Failed to load file', 'error');
                };
                reader.readAsText(file);
            }
            e.target.value = '';
        });

        // Copy output
        document.getElementById('copyOutputBtn').addEventListener('click', () => {
            const output = document.getElementById('outputData');
            if (output.value) {
                navigator.clipboard.writeText(output.value).then(() => {
                    this.showLog('📋 Result copied to clipboard', 'success');
                }).catch(() => {
                    output.select();
                    document.execCommand('copy');
                    this.showLog('📋 Result copied', 'success');
                });
            } else {
                this.showLog('⚠️ No data to copy', 'error');
            }
        });

        // Save output
        document.getElementById('saveOutputBtn').addEventListener('click', () => {
            this.saveOutput();
        });

        // Clear input
        document.getElementById('clearInputBtn').addEventListener('click', () => {
            document.getElementById('inputData').value = '';
            this.showLog('🗑️ Input cleared', 'info');
            this.saveRecipeToStorage();
            this.triggerAutoBake();
        });

        // Clear output
        document.getElementById('clearOutputBtn').addEventListener('click', () => {
            document.getElementById('outputData').value = '';
            this.clearLog();
        });

        // Manual execute
        document.getElementById('executeBtn').addEventListener('click', () => {
            this.executeRecipe();
        });

        // Auto-execute on input change
        document.getElementById('inputData').addEventListener('input', () => {
            this.saveRecipeToStorage();
            this.triggerAutoBake();
        });

        // Auto-bake toggle
        document.getElementById('autoBakeToggle').addEventListener('change', (e) => {
            this.autoBakeEnabled = e.target.checked;
            this.updateAutoBakeUI();
            this.saveRecipeToStorage();

            if (this.autoBakeEnabled) {
                this.showLog('🔄 Auto-execution enabled', 'success');
                this.triggerAutoBake();
            } else {
                this.showLog('⏸️ Auto-execution disabled', 'info');
                document.getElementById('executeBtn').style.display = 'inline-block';
            }
        });

        console.log('✅ Event listeners ready');
    }

    updateAutoBakeUI() {
        const toggle = document.getElementById('autoBakeToggle');
        const label = document.getElementById('autoBakeLabel');
        const executeBtn = document.getElementById('executeBtn');

        toggle.checked = this.autoBakeEnabled;
        label.textContent = this.autoBakeEnabled ? 'Auto' : 'Manual';
        label.className = this.autoBakeEnabled ? 'active' : 'inactive';
        executeBtn.style.display = this.autoBakeEnabled ? 'none' : 'inline-block';
    }

    renderOperations(filter = '') {
        const container = document.getElementById('operationsList');
        if (!container) return;

        const filtered = this.operations.filter(op =>
            op.name.toLowerCase().includes(filter.toLowerCase()) ||
            op.category.toLowerCase().includes(filter.toLowerCase()) ||
            op.description.toLowerCase().includes(filter.toLowerCase())
        );

        if (filtered.length === 0) {
            container.innerHTML = '<div class="empty-recipe">🔍 No operations found</div>';
            return;
        }

        const grouped = filtered.reduce((acc, op) => {
            if (!acc[op.category]) acc[op.category] = [];
            acc[op.category].push(op);
            return acc;
        }, {});

        container.innerHTML = Object.entries(grouped).map(([category, ops]) => `
            <div class="operation-category">
                <div class="category-title">${category}</div>
                ${ops.map(op => `
                    <div class="operation-item" data-operation="${op.name}">
                        <span class="op-name">${op.name}</span>
                        <span class="op-desc">${op.description}</span>
                    </div>
                `).join('')}
            </div>
        `).join('');
    }

    addToRecipe(operation) {
        const parameters = {};
        if (operation.parameters && operation.parameters.length > 0) {
            operation.parameters.forEach(paramInfo => {
                if (paramInfo.defaultValue !== undefined && paramInfo.defaultValue !== null) {
                    parameters[paramInfo.name] = paramInfo.defaultValue;
                } else {
                    switch (paramInfo.type) {
                        case 'number':
                        case 'byte':
                            parameters[paramInfo.name] = 0;
                            break;
                        case 'boolean':
                            parameters[paramInfo.name] = false;
                            break;
                        case 'select':
                            parameters[paramInfo.name] = paramInfo.options && paramInfo.options.length > 0
                                ? paramInfo.options[0]
                                : '';
                            break;
                        default:
                            parameters[paramInfo.name] = '';
                    }
                }
            });
        }

        this.recipe.push({
            operationName: operation.name,
            parameters: parameters
        });

        this.renderRecipe();
        this.saveRecipeToStorage();
        this.showLog(`➕ Added operation: ${operation.name}`, 'success');
        this.triggerAutoBake();
    }

    removeFromRecipe(index) {
        const removed = this.recipe[index];
        this.recipe.splice(index, 1);
        this.renderRecipe();
        this.saveRecipeToStorage();
        this.showLog(`➖ Removed operation: ${removed.operationName}`, 'info');
        this.triggerAutoBake();
    }

    renderRecipe() {
        const container = document.getElementById('recipeList');
        if (!container) return;

        if (this.recipe.length === 0) {
            container.innerHTML = '<div class="empty-recipe">🔄 Drag operations here</div>';
            return;
        }

        container.innerHTML = this.recipe.map((step, index) => {
            const operation = this.operations.find(op => op.name === step.operationName);
            const paramTypes = operation?.parameters || [];

            return `
                <div class="recipe-step" data-step-index="${index}">
                    <span class="step-number">#${index + 1}</span>
                    <span class="step-name">${step.operationName}</span>
                    <button class="remove-step" data-index="${index}">✕</button>
                    <div class="step-params">
                        ${Object.entries(step.parameters).map(([key, value]) => {
                const paramInfo = paramTypes.find(p => p.name === key);
                const paramType = paramInfo?.type || 'string';
                const options = paramInfo?.options || [];
                const description = paramInfo?.description || '';
                const label = paramInfo?.label || key;

                let inputHtml = '';
                if (paramType === 'select' && options.length > 0) {
                    inputHtml = `
                                    <select data-step-index="${index}" data-param-name="${key}" class="param-select" title="${description}">
                                        ${options.map(opt => `
                                            <option value="${opt}" ${value === opt ? 'selected' : ''}>${opt}</option>
                                        `).join('')}
                                    </select>
                                `;
                } else if (paramType === 'number' || paramType === 'byte') {
                    inputHtml = `
                                    <input type="number" 
                                           value="${value}" 
                                           data-step-index="${index}" 
                                           data-param-name="${key}"
                                           class="param-input"
                                           step="${paramType === 'byte' ? '1' : 'any'}"
                                           min="${paramType === 'byte' ? '0' : ''}"
                                           max="${paramType === 'byte' ? '255' : ''}"
                                           title="${description}">
                                `;
                } else if (paramType === 'boolean') {
                    inputHtml = `
                                    <input type="checkbox" 
                                           ${value ? 'checked' : ''}
                                           data-step-index="${index}" 
                                           data-param-name="${key}"
                                           class="param-checkbox"
                                           title="${description}">
                                `;
                } else {
                    inputHtml = `
                                    <input type="text" 
                                           value="${value}" 
                                           data-step-index="${index}" 
                                           data-param-name="${key}"
                                           class="param-input"
                                           placeholder="Value"
                                           title="${description}">
                                `;
                }

                if (paramType === 'boolean') {
                    return `
                                    <label title="${description}">
                                        ${inputHtml}
                                        <span class="param-label">${label}</span>
                                    </label>
                                `;
                }

                return `
                                <label title="${description}">
                                    <span class="param-label">${label}</span>
                                    ${inputHtml}
                                </label>
                            `;
            }).join('')}
                    </div>
                </div>
            `;
        }).join('');

        this.bindParameterEvents();
    }

    bindParameterEvents() {
        document.querySelectorAll('.param-input').forEach(input => {
            input.removeEventListener('input', this.handleParamChange);
            input.removeEventListener('change', this.handleParamChange);
            input.addEventListener('input', this.handleParamChange.bind(this));
            input.addEventListener('change', this.handleParamChange.bind(this));
        });

        document.querySelectorAll('.param-select').forEach(select => {
            select.removeEventListener('change', this.handleParamChange);
            select.addEventListener('change', this.handleParamChange.bind(this));
        });

        document.querySelectorAll('.param-checkbox').forEach(checkbox => {
            checkbox.removeEventListener('change', this.handleParamChange);
            checkbox.addEventListener('change', this.handleParamChange.bind(this));
        });
    }

    handleParamChange(e) {
        const target = e.target;
        const index = parseInt(target.dataset.stepIndex);
        const paramName = target.dataset.paramName;

        if (isNaN(index) || !paramName) return;

        let value;
        if (target.type === 'checkbox') {
            value = target.checked;
        } else if (target.tagName === 'SELECT') {
            value = target.value;
        } else if (target.type === 'number') {
            value = parseFloat(target.value);
            if (isNaN(value)) value = 0;
        } else {
            value = target.value;
        }

        if (this.recipe[index]) {
            this.recipe[index].parameters[paramName] = value;
            this.saveRecipeToStorage();
            this.triggerAutoBake();
        }
    }

    saveRecipeToStorage() {
        try {
            const recipeData = {
                recipe: this.recipe,
                autoBakeEnabled: this.autoBakeEnabled,
                inputData: document.getElementById('inputData').value || ''
            };
            localStorage.setItem('cyberchef_recipe', JSON.stringify(recipeData));
        } catch (error) {
            console.warn('⚠️ Failed to save recipe:', error);
        }
    }

    loadRecipeFromStorage() {
        try {
            const saved = localStorage.getItem('cyberchef_recipe');
            if (!saved) {
                console.log('📭 No saved recipe found');
                return;
            }

            const recipeData = JSON.parse(saved);

            if (recipeData.recipe && Array.isArray(recipeData.recipe) && recipeData.recipe.length > 0) {
                this.recipe = recipeData.recipe;
                console.log(`📂 Loaded recipe (${this.recipe.length} operations)`);
            }

            if (recipeData.autoBakeEnabled !== undefined) {
                this.autoBakeEnabled = recipeData.autoBakeEnabled;
            }

            if (recipeData.inputData !== undefined) {
                document.getElementById('inputData').value = recipeData.inputData;
            }
        } catch (error) {
            console.warn('⚠️ Failed to load recipe:', error);
        }
    }

    clearRecipeFromStorage() {
        try {
            localStorage.removeItem('cyberchef_recipe');
        } catch (error) {
            console.warn('⚠️ Failed to clear recipe:', error);
        }
    }

    triggerAutoBake() {
        if (!this.autoBakeEnabled) return;

        if (this.autoBakeTimeout) {
            clearTimeout(this.autoBakeTimeout);
        }

        const input = document.getElementById('inputData').value;
        if (!input || this.recipe.length === 0) {
            if (!input) {
                document.getElementById('outputData').value = '';
            }
            return;
        }

        this.autoBakeTimeout = setTimeout(() => {
            this.executeRecipe();
        }, 500);
    }

    async executeRecipe() {
        if (this.isExecuting) return;

        const input = document.getElementById('inputData').value;
        if (this.recipe.length === 0) return;

        this.syncParametersFromDOM();

        this.isExecuting = true;
        document.getElementById('outputData').value = '⏳ Executing...';

        try {
            const steps = this.recipe.map(step => {
                const operation = this.operations.find(op => op.name === step.operationName);
                const parameters = {};

                Object.entries(step.parameters).forEach(([key, value]) => {
                    const paramType = this.getParameterType(operation, key);

                    if (paramType === 'number' || paramType === 'byte') {
                        const num = parseFloat(value);
                        parameters[key] = isNaN(num) ? 0 : num;
                    } else if (paramType === 'boolean') {
                        parameters[key] = value === true || value === 'true';
                    } else {
                        parameters[key] = value || '';
                    }
                });

                return {
                    operationName: step.operationName,
                    parameters: parameters
                };
            });

            const inputBase64 = input ? btoa(unescape(encodeURIComponent(input))) : '';

            const request = {
                inputBase64: inputBase64,
                steps: steps
            };

            const response = await fetch(`${this.baseUrl}/execute`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    'Accept': 'application/json; charset=utf-8'
                },
                body: JSON.stringify(request)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const result = await response.json();

            if (result.success) {
                try {
                    const output = atob(result.finalOutputBase64);
                    document.getElementById('outputData').value = decodeURIComponent(escape(output));
                } catch {
                    document.getElementById('outputData').value = result.finalOutputBase64 || 'Empty result';
                }
            } else {
                const errorMsg = result.errors?.join(', ') || 'Unknown error';
                document.getElementById('outputData').value = `❌ Error: ${errorMsg}`;
                this.showLog(`❌ ${errorMsg}`, 'error');
            }

            if (result.steps) {
                this.renderStepLog(result.steps);
            }

        } catch (error) {
            console.error('Execution error:', error);
            document.getElementById('outputData').value = `❌ Network error: ${error.message}`;
            this.showLog(`❌ ${error.message}`, 'error');
        } finally {
            this.isExecuting = false;
        }
    }

    syncParametersFromDOM() {
        document.querySelectorAll('.recipe-step').forEach((stepElement, index) => {
            if (!this.recipe[index]) return;

            stepElement.querySelectorAll('.param-input').forEach(input => {
                const paramName = input.dataset.paramName;
                if (!paramName) return;

                let value;
                if (input.type === 'number') {
                    value = parseFloat(input.value);
                    if (isNaN(value)) value = 0;
                } else {
                    value = input.value;
                }
                this.recipe[index].parameters[paramName] = value;
            });

            stepElement.querySelectorAll('.param-select').forEach(select => {
                const paramName = select.dataset.paramName;
                if (!paramName) return;
                this.recipe[index].parameters[paramName] = select.value;
            });

            stepElement.querySelectorAll('.param-checkbox').forEach(checkbox => {
                const paramName = checkbox.dataset.paramName;
                if (!paramName) return;
                this.recipe[index].parameters[paramName] = checkbox.checked;
            });
        });
    }

    getParameterType(operation, paramName) {
        if (operation?.parameters) {
            const paramInfo = operation.parameters.find(p => p.name === paramName);
            if (paramInfo) return paramInfo.type;
        }

        const numberParams = ['Key', 'Shift', 'Iterations', 'Amount', 'Length', 'Size'];
        const byteParams = ['KeyByte'];
        const boolParams = ['UsePadding', 'StripPadding', 'UseSalt', 'UseIV'];

        if (byteParams.includes(paramName)) return 'byte';
        if (numberParams.includes(paramName)) return 'number';
        if (boolParams.includes(paramName)) return 'boolean';
        if (paramName.includes('Format') || paramName.includes('Mode') || paramName.includes('Encoding')) {
            if (operation?.parameters) {
                const paramInfo = operation.parameters.find(p => p.name === paramName);
                if (paramInfo?.options && paramInfo.options.length > 0) {
                    return 'select';
                }
            }
        }
        return 'string';
    }

    renderStepLog(steps) {
        const logArea = document.getElementById('executionLog');
        if (!logArea) return;

        const errorStep = steps.find(s => !s.success);
        if (errorStep) {
            logArea.innerHTML = `
                <div class="log-entry error">
                    ❌ Error at step ${errorStep.stepIndex + 1}: ${errorStep.error}
                </div>
            `;
        } else {
            logArea.innerHTML = `
                <div class="log-entry success">
                    ✅ Executed ${steps.length} steps successfully
                </div>
            `;
        }

        logArea.scrollTop = logArea.scrollHeight;
    }

    saveOutput() {
        const output = document.getElementById('outputData').value;
        if (!output) {
            this.showLog('⚠️ No data to save', 'error');
            return;
        }

        try {
            const now = new Date();
            const timestamp = this.formatTimestamp(now);
            const filename = `cyberchef_result_${timestamp}.txt`;

            const blob = new Blob([output], { type: 'text/plain;charset=utf-8' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = filename;

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            setTimeout(() => {
                URL.revokeObjectURL(url);
            }, 100);

            this.showLog(`💾 Result saved to: ${filename}`, 'success');
        } catch (error) {
            console.error('Save error:', error);
            this.showLog(`❌ Save error: ${error.message}`, 'error');
        }
    }

    formatTimestamp(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        return `${year}-${month}-${day}_${hours}-${minutes}-${seconds}`;
    }

    showLog(message, type = 'info') {
        const logArea = document.getElementById('executionLog');
        if (!logArea) return;

        const entry = document.createElement('div');
        entry.className = `log-entry ${type}`;
        entry.textContent = message;
        logArea.appendChild(entry);

        while (logArea.children.length > 100) {
            logArea.removeChild(logArea.firstChild);
        }

        logArea.scrollTop = logArea.scrollHeight;
    }

    clearLog() {
        const logArea = document.getElementById('executionLog');
        if (logArea) {
            logArea.innerHTML = '';
        }
    }

    showError(message) {
        const output = document.getElementById('outputData');
        if (output) {
            output.value = `❌ ${message}`;
        }
        this.showLog(message, 'error');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    console.log('🚀 CyberChef Clone initializing...');
    window.app = new CyberChefClient();
    console.log('✅ Ready!');
});