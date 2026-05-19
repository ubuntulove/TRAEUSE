export class Button {
    constructor(options = {}) {
        this.id = options.id || 'btn_' + Date.now();
        this.text = options.text || 'Button';
        this.variant = options.variant || 'primary';
        this.size = options.size || 'medium';
        this.icon = options.icon || null;
        this.iconPosition = options.iconPosition || 'left';
        this.disabled = options.disabled || false;
        this.loading = options.loading || false;
        this.fullWidth = options.fullWidth || false;
        this.type = options.type || 'button';
        this.classes = options.classes || [];
        this.styles = options.styles || {};
        this.tooltip = options.tooltip || null;
        this.badge = options.badge || null;
        this.dropdown = options.dropdown || null;
        this.state = 'default';
        this.onClick = options.onClick || null;
        this.onMouseEnter = options.onMouseEnter || null;
        this.onMouseLeave = options.onMouseLeave || null;
    }

    render() {
        const classes = [
            'btn',
            `btn-${this.variant}`,
            `btn-${this.size}`,
            ...this.classes,
            this.disabled ? 'btn-disabled' : '',
            this.loading ? 'btn-loading' : '',
            this.fullWidth ? 'btn-full-width' : ''
        ].filter(Boolean).join(' ');

        const style = {
            ...this.styles
        };

        const props = {
            id: this.id,
            class: classes,
            style,
            type: this.type,
            disabled: this.disabled || this.loading,
            'data-state': this.state
        };

        if (this.tooltip) {
            props['data-tooltip'] = this.tooltip;
        }

        let html = `<button`;

        for (const [key, value] of Object.entries(props)) {
            if (value !== null && value !== undefined && value !== false) {
                html += ` ${key}="${value}"`;
            }
        }

        html += `>`;

        if (this.loading) {
            html += `<span class="btn-spinner"></span>`;
        }

        if (this.icon && this.iconPosition === 'left') {
            html += `<span class="btn-icon">${this.icon}</span>`;
        }

        html += `<span class="btn-text">${this.text}</span>`;

        if (this.icon && this.iconPosition === 'right') {
            html += `<span class="btn-icon">${this.icon}</span>`;
        }

        if (this.badge) {
            html += `<span class="btn-badge">${this.badge}</span>`;
        }

        if (this.dropdown) {
            html += `<span class="btn-dropdown-icon">▼</span>`;
        }

        html += `</button>`;

        return html;
    }

    setState(state) {
        this.state = state;
    }

    setLoading(loading) {
        this.loading = loading;
    }

    setDisabled(disabled) {
        this.disabled = disabled;
    }

    setText(text) {
        this.text = text;
    }
}

export class Input {
    constructor(options = {}) {
        this.id = options.id || 'input_' + Date.now();
        this.type = options.type || 'text';
        this.name = options.name || '';
        this.value = options.value || '';
        this.placeholder = options.placeholder || '';
        this.label = options.label || null;
        this.hint = options.hint || null;
        this.error = options.error || null;
        this.size = options.size || 'medium';
        this.disabled = options.disabled || false;
        this.readonly = options.readonly || false;
        this.required = options.required || false;
        this.autocomplete = options.autocomplete || null;
        this.autofocus = options.autofocus || false;
        this.maxlength = options.maxlength || null;
        this.min = options.min || null;
        this.max = options.max || null;
        this.step = options.step || null;
        this.pattern = options.pattern || null;
        this.icon = options.icon || null;
        this.iconPosition = options.iconPosition || 'left';
        this.clearable = options.clearable || false;
        this.classes = options.classes || [];
        this.styles = options.styles || {};
        this.validation = options.validation || null;
        this.onChange = options.onChange || null;
        this.onInput = options.onInput || null;
        this.onFocus = options.onFocus || null;
        this.onBlur = options.onBlur || null;
    }

    render() {
        const wrapperClasses = [
            'input-wrapper',
            `input-${this.size}`,
            this.error ? 'input-error' : '',
            this.disabled ? 'input-disabled' : '',
            this.icon ? `input-with-icon-${this.iconPosition}` : '',
            this.clearable ? 'input-clearable' : ''
        ].filter(Boolean).join(' ');

        let html = `<div class="${wrapperClasses}" style="${this.styles.wrapper || ''}">`;

        if (this.label) {
            html += `<label for="${this.id}" class="input-label">
                ${this.label}
                ${this.required ? '<span class="input-required">*</span>' : ''}
            </label>`;
        }

        html += `<div class="input-container">`;

        if (this.icon && this.iconPosition === 'left') {
            html += `<span class="input-icon">${this.icon}</span>`;
        }

        const inputClasses = [
            'input-field',
            ...this.classes,
            this.error ? 'input-field-error' : ''
        ].filter(Boolean).join(' ');

        html += `<input
            id="${this.id}"
            type="${this.type}"
            name="${this.name}"
            value="${this.value}"
            placeholder="${this.placeholder}"
            class="${inputClasses}"
            ${this.disabled ? 'disabled' : ''}
            ${this.readonly ? 'readonly' : ''}
            ${this.required ? 'required' : ''}
            ${this.autocomplete ? `autocomplete="${this.autocomplete}"` : ''}
            ${this.autofocus ? 'autofocus' : ''}
            ${this.maxlength ? `maxlength="${this.maxlength}"` : ''}
            ${this.min !== null ? `min="${this.min}"` : ''}
            ${this.max !== null ? `max="${this.max}"` : ''}
            ${this.step !== null ? `step="${this.step}"` : ''}
            ${this.pattern ? `pattern="${this.pattern}"` : ''}
        />`;

        if (this.icon && this.iconPosition === 'right') {
            html += `<span class="input-icon">${this.icon}</span>`;
        }

        if (this.clearable && this.value) {
            html += `<button type="button" class="input-clear-btn" data-clear="${this.id}">×</button>`;
        }

        html += `</div>`;

        if (this.error) {
            html += `<span class="input-error-message">${this.error}</span>`;
        } else if (this.hint) {
            html += `<span class="input-hint">${this.hint}</span>`;
        }

        html += `</div>`;

        return html;
    }

    validate() {
        if (this.required && !this.value) {
            this.error = 'This field is required';
            return false;
        }

        if (this.pattern && !new RegExp(this.pattern).test(this.value)) {
            this.error = 'Invalid format';
            return false;
        }

        if (this.min !== null && parseFloat(this.value) < this.min) {
            this.error = `Minimum value is ${this.min}`;
            return false;
        }

        if (this.max !== null && parseFloat(this.value) > this.max) {
            this.error = `Maximum value is ${this.max}`;
            return false;
        }

        if (this.validation && typeof this.validation === 'function') {
            const result = this.validation(this.value);
            if (result !== true) {
                this.error = result;
                return false;
            }
        }

        this.error = null;
        return true;
    }

    setValue(value) {
        this.value = value;
    }

    getValue() {
        return this.value;
    }

    clear() {
        this.value = '';
        this.error = null;
    }
}

export class Select {
    constructor(options = {}) {
        this.id = options.id || 'select_' + Date.now();
        this.name = options.name || '';
        this.value = options.value || '';
        this.placeholder = options.placeholder || 'Select...';
        this.options = options.options || [];
        this.groups = options.groups || [];
        this.label = options.label || null;
        this.hint = options.hint || null;
        this.error = options.error || null;
        this.size = options.size || 'medium';
        this.disabled = options.disabled || false;
        this.required = options.required || false;
        this.multiple = options.multiple || false;
        this.searchable = options.searchable || false;
        this.clearable = options.clearable || false;
        this.selected = options.selected || [];
        this.classes = options.classes || [];
        this.styles = options.styles || {};
        this.onChange = options.onChange || null;
    }

    render() {
        const wrapperClasses = [
            'select-wrapper',
            `select-${this.size}`,
            this.error ? 'select-error' : '',
            this.disabled ? 'select-disabled' : '',
            this.searchable ? 'select-searchable' : '',
            this.multiple ? 'select-multiple' : ''
        ].filter(Boolean).join(' ');

        let html = `<div class="${wrapperClasses}">`;

        if (this.label) {
            html += `<label for="${this.id}" class="select-label">${this.label}</label>`;
        }

        html += `<div class="select-container">`;

        html += `<select
            id="${this.id}"
            name="${this.name}"
            class="select-field"
            ${this.disabled ? 'disabled' : ''}
            ${this.required ? 'required' : ''}
            ${this.multiple ? 'multiple' : ''}
        >`;

        if (this.placeholder && !this.multiple) {
            html += `<option value="" disabled ${!this.value ? 'selected' : ''}>
                ${this.placeholder}
            </option>`;
        }

        for (const option of this.options) {
            const selected = this.multiple 
                ? this.selected.includes(option.value)
                : this.value === option.value;
            
            html += `<option 
                value="${option.value}"
                ${selected ? 'selected' : ''}
                ${option.disabled ? 'disabled' : ''}
            >
                ${option.label}
                ${option.description ? ` - ${option.description}` : ''}
            </option>`;
        }

        for (const group of this.groups) {
            html += `<optgroup label="${group.label}">`;
            
            for (const option of group.options) {
                const selected = this.multiple 
                    ? this.selected.includes(option.value)
                    : this.value === option.value;
                
                html += `<option 
                    value="${option.value}"
                    ${selected ? 'selected' : ''}
                >
                    ${option.label}
                </option>`;
            }
            
            html += `</optgroup>`;
        }

        html += `</select>`;

        if (this.clearable && (this.value || this.selected.length > 0)) {
            html += `<button type="button" class="select-clear-btn">×</button>`;
        }

        html += `<span class="select-arrow">▼</span>`;
        html += `</div>`;

        if (this.error) {
            html += `<span class="select-error-message">${this.error}</span>`;
        }

        html += `</div>`;

        return html;
    }

    setValue(value) {
        this.value = value;
    }

    getValue() {
        return this.multiple ? this.selected : this.value;
    }

    addOption(option) {
        this.options.push(option);
    }

    removeOption(value) {
        this.options = this.options.filter(o => o.value !== value);
    }
}

export class Modal {
    constructor(options = {}) {
        this.id = options.id || 'modal_' + Date.now();
        this.title = options.title || '';
        this.content = options.content || '';
        this.footer = options.footer || null;
        this.size = options.size || 'medium';
        this.closable = options.closable ?? true;
        this.closeOnOverlay = options.closeOnOverlay ?? true;
        this.closeOnEscape = options.closeOnEscape ?? true;
        this.showCloseButton = options.showCloseButton ?? true;
        this.visible = false;
        this.animation = options.animation || 'fade';
        this.classes = options.classes || [];
        this.styles = options.styles || {};
        this.onClose = options.onClose || null;
        this.onOpen = options.onOpen || null;
    }

    render() {
        const classes = [
            'modal-overlay',
            this.visible ? 'modal-visible' : 'modal-hidden',
            `modal-animation-${this.animation}`,
            ...this.classes
        ].filter(Boolean).join(' ');

        let html = `<div id="${this.id}" class="${classes}">`;

        html += `<div class="modal-container modal-${this.size}">`;

        html += `<div class="modal-header">`;
        html += `<h2 class="modal-title">${this.title}</h2>`;
        
        if (this.showCloseButton && this.closable) {
            html += `<button type="button" class="modal-close-btn" data-close-modal="${this.id}">×</button>`;
        }
        
        html += `</div>`;

        html += `<div class="modal-body">`;
        html += typeof this.content === 'function' ? this.content() : this.content;
        html += `</div>`;

        if (this.footer) {
            html += `<div class="modal-footer">`;
            html += typeof this.footer === 'function' ? this.footer() : this.footer;
            html += `</div>`;
        }

        html += `</div>`;
        html += `</div>`;

        return html;
    }

    open() {
        this.visible = true;
        if (this.onOpen) this.onOpen();
    }

    close() {
        this.visible = false;
        if (this.onClose) this.onClose();
    }

    toggle() {
        this.visible ? this.close() : this.open();
    }

    setContent(content) {
        this.content = content;
    }

    setTitle(title) {
        this.title = title;
    }
}

export class Card {
    constructor(options = {}) {
        this.id = options.id || 'card_' + Date.now();
        this.title = options.title || null;
        this.subtitle = options.subtitle || null;
        this.content = options.content || '';
        this.footer = options.footer || null;
        this.header = options.header || null;
        this.image = options.image || null;
        this.avatar = options.avatar || null;
        this.href = options.href || null;
        this.clickable = options.clickable ?? false;
        this.hoverable = options.hoverable ?? true;
        this.padding = options.padding || 'medium';
        this.border = options.border ?? true;
        this.shadow = options.shadow || 'medium';
        this.radius = options.radius || 'medium';
        this.classes = options.classes || [];
        this.styles = options.styles || {};
        this.tags = options.tags || [];
        this.badge = options.badge || null;
        this.meta = options.meta || null;
        this.actions = options.actions || [];
    }

    render() {
        const classes = [
            'card',
            `card-padding-${this.padding}`,
            `card-shadow-${this.shadow}`,
            `card-radius-${this.radius}`,
            this.border ? 'card-bordered' : 'card-borderless',
            this.hoverable ? 'card-hoverable' : '',
            this.clickable ? 'card-clickable' : '',
            ...this.classes
        ].filter(Boolean).join(' ');

        let html = '';

        if (this.href) {
            html += `<a href="${this.href}" class="${classes}" id="${this.id}">`;
        } else {
            html += `<div class="${classes}" id="${this.id}">`;
        }

        if (this.image) {
            html += `<div class="card-image">
                <img src="${this.image.src}" alt="${this.image.alt || ''}" />
                ${this.badge ? `<span class="card-badge">${this.badge}</span>` : ''}
            </div>`;
        }

        if (this.header || this.avatar || this.title) {
            html += `<div class="card-header">`;
            
            if (this.avatar) {
                html += `<img class="card-avatar" src="${this.avatar.src}" alt="${this.avatar.alt || ''}" />`;
            }

            if (this.title || this.subtitle) {
                html += `<div class="card-header-text">`;
                if (this.title) {
                    html += `<h3 class="card-title">${this.title}</h3>`;
                }
                if (this.subtitle) {
                    html += `<p class="card-subtitle">${this.subtitle}</p>`;
                }
                html += `</div>`;
            }

            if (this.header) {
                html += this.header;
            }

            html += `</div>`;
        }

        if (this.meta || this.tags.length > 0) {
            html += `<div class="card-meta">`;
            
            if (this.meta) {
                html += `<span class="card-meta-text">${this.meta}</span>`;
            }

            for (const tag of this.tags) {
                html += `<span class="card-tag">${tag}</span>`;
            }

            html += `</div>`;
        }

        html += `<div class="card-body">`;
        html += typeof this.content === 'function' ? this.content() : this.content;
        html += `</div>`;

        if (this.footer || this.actions.length > 0) {
            html += `<div class="card-footer">`;
            
            if (this.footer) {
                html += typeof this.footer === 'function' ? this.footer() : this.footer;
            }

            if (this.actions.length > 0) {
                html += `<div class="card-actions">`;
                for (const action of this.actions) {
                    html += `<button type="button" class="card-action-btn" data-action="${action.action}">
                        ${action.icon || ''} ${action.label}
                    </button>`;
                }
                html += `</div>`;
            }

            html += `</div>`;
        }

        if (this.href) {
            html += `</a>`;
        } else {
            html += `</div>`;
        }

        return html;
    }
}

export class Table {
    constructor(options = {}) {
        this.id = options.id || 'table_' + Date.now();
        this.columns = options.columns || [];
        this.data = options.data || [];
        this.sortable = options.sortable ?? true;
        this.filterable = options.filterable ?? true;
        this.pagination = options.pagination ?? true;
        this.pageSize = options.pageSize || 10;
        this.currentPage = 1;
        this.selectedRows = [];
        this.selectable = options.selectable ?? false;
        this.expandable = options.expandable ?? false;
        this.striped = options.striped ?? false;
        this.bordered = options.bordered ?? true;
        this.hoverable = options.hoverable ?? true;
        this.compact = options.compact ?? false;
        this.sortColumn = null;
        this.sortDirection = 'asc';
        this.filterQuery = '';
        this.classes = options.classes || [];
        this.onRowClick = options.onRowClick || null;
        this.onSort = options.onSort || null;
        this.onFilter = options.onFilter || null;
        this.emptyText = options.emptyText || 'No data available';
    }

    render() {
        let html = `<div class="table-container" id="${this.id}">`;

        if (this.filterable) {
            html += `<div class="table-toolbar">`;
            html += `<input type="text" class="table-filter-input" placeholder="Filter..." value="${this.filterQuery}" />`;
            
            if (this.pagination) {
                html += `<div class="table-pagination-info">
                    Showing ${this.getStartIndex() + 1} to ${this.getEndIndex()} of ${this.getFilteredData().length}
                </div>`;
            }
            
            html += `</div>`;
        }

        html += `<table class="table ${this.classes.join(' ')}">`;

        html += `<thead class="table-thead">`;
        html += `<tr>`;

        if (this.selectable) {
            html += `<th class="table-th table-th-checkbox">
                <input type="checkbox" class="table-checkbox" data-select-all="true" />
            </th>`;
        }

        for (const column of this.columns) {
            const sortClass = this.sortColumn === column.key ? `table-th-sort-${this.sortDirection}` : '';
            const sortableClass = this.sortable && column.sortable !== false ? 'table-th-sortable' : '';
            
            html += `<th class="table-th ${sortableClass} ${sortClass}"
                ${column.width ? `style="width: ${column.width}"` : ''}
                ${column.align ? `style="text-align: ${column.align}"` : ''}
                ${column.key ? `data-column="${column.key}"` : ''}
            >
                ${column.label}
                ${this.sortable && column.sortable !== false ? `<span class="table-sort-icon">↕</span>` : ''}
            </th>`;
        }

        html += `</tr>`;
        html += `</thead>`;

        html += `<tbody class="table-tbody">`;

        const displayData = this.getDisplayData();

        if (displayData.length === 0) {
            const colCount = this.columns.length + (this.selectable ? 1 : 0);
            html += `<tr class="table-empty-row">
                <td colspan="${colCount}" class="table-empty-cell">
                    ${this.emptyText}
                </td>
            </tr>`;
        } else {
            for (const row of displayData) {
                const selectedClass = this.selectedRows.includes(row.id) ? 'table-row-selected' : '';
                html += `<tr class="table-tr ${selectedClass}" data-row-id="${row.id}">`;

                if (this.selectable) {
                    html += `<td class="table-td table-td-checkbox">
                        <input type="checkbox" class="table-checkbox" 
                            data-select-row="${row.id}"
                            ${this.selectedRows.includes(row.id) ? 'checked' : ''}
                        />
                    </td>`;
                }

                for (const column of this.columns) {
                    const value = this.getCellValue(row, column);
                    const cellClass = typeof column.cellClass === 'function' 
                        ? column.cellClass(value, row)
                        : column.cellClass || '';
                    
                    html += `<td class="table-td ${cellClass}"
                        ${column.align ? `style="text-align: ${column.align}"` : ''}
                    >
                        ${typeof column.render === 'function' ? column.render(value, row) : value}
                    </td>`;
                }

                html += `</tr>`;
            }
        }

        html += `</tbody>`;
        html += `</table>`;

        if (this.pagination) {
            html += this.renderPagination();
        }

        html += `</div>`;

        return html;
    }

    renderPagination() {
        const totalPages = Math.ceil(this.getFilteredData().length / this.pageSize);
        
        let html = `<div class="table-pagination">`;

        html += `<button type="button" class="pagination-btn" 
            data-page="prev" ${this.currentPage === 1 ? 'disabled' : ''}>
            Previous
        </button>`;

        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || (i >= this.currentPage - 1 && i <= this.currentPage + 1)) {
                html += `<button type="button" class="pagination-btn ${i === this.currentPage ? 'active' : ''}"
                    data-page="${i}">
                    ${i}
                </button>`;
            } else if (i === this.currentPage - 2 || i === this.currentPage + 2) {
                html += `<span class="pagination-ellipsis">...</span>`;
            }
        }

        html += `<button type="button" class="pagination-btn"
            data-page="next" ${this.currentPage === totalPages ? 'disabled' : ''}>
            Next
        </button>`;

        html += `</div>`;

        return html;
    }

    getFilteredData() {
        if (!this.filterQuery) return this.data;

        const query = this.filterQuery.toLowerCase();
        return this.data.filter(row => {
            return this.columns.some(column => {
                const value = this.getCellValue(row, column);
                return String(value).toLowerCase().includes(query);
            });
        });
    }

    getSortedData() {
        if (!this.sortColumn) return this.getFilteredData();

        const data = [...this.getFilteredData()];
        const column = this.columns.find(c => c.key === this.sortColumn);

        data.sort((a, b) => {
            const aVal = this.getCellValue(a, column);
            const bVal = this.getCellValue(b, column);

            if (aVal < bVal) return this.sortDirection === 'asc' ? -1 : 1;
            if (aVal > bVal) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });

        return data;
    }

    getDisplayData() {
        let data = this.getSortedData();

        if (this.pagination) {
            const start = (this.currentPage - 1) * this.pageSize;
            data = data.slice(start, start + this.pageSize);
        }

        return data;
    }

    getCellValue(row, column) {
        if (column.accessor) {
            return typeof column.accessor === 'function' 
                ? column.accessor(row) 
                : row[column.accessor];
        }
        return row[column.key];
    }

    getStartIndex() {
        return (this.currentPage - 1) * this.pageSize;
    }

    getEndIndex() {
        return Math.min(this.currentPage * this.pageSize, this.getFilteredData().length);
    }

    sort(columnKey) {
        if (this.sortColumn === columnKey) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = columnKey;
            this.sortDirection = 'asc';
        }

        if (this.onSort) {
            this.onSort(this.sortColumn, this.sortDirection);
        }
    }

    filter(query) {
        this.filterQuery = query;
        this.currentPage = 1;

        if (this.onFilter) {
            this.onFilter(query);
        }
    }

    setPage(page) {
        const totalPages = Math.ceil(this.getFilteredData().length / this.pageSize);
        if (page >= 1 && page <= totalPages) {
            this.currentPage = page;
        }
    }

    selectRow(rowId) {
        if (!this.selectedRows.includes(rowId)) {
            this.selectedRows.push(rowId);
        }
    }

    deselectRow(rowId) {
        this.selectedRows = this.selectedRows.filter(id => id !== rowId);
    }

    selectAll() {
        this.selectedRows = this.getDisplayData().map(row => row.id);
    }

    deselectAll() {
        this.selectedRows = [];
    }
}

export class Tabs {
    constructor(options = {}) {
        this.id = options.id || 'tabs_' + Date.now();
        this.tabs = options.tabs || [];
        this.activeTab = options.activeTab || null;
        this.orientation = options.orientation || 'horizontal';
        this.variant = options.variant || 'default';
        this.animated = options.animated ?? true;
        this.lazy = options.lazy ?? false;
        this.onChange = options.onChange || null;
    }

    render() {
        if (!this.activeTab && this.tabs.length > 0) {
            this.activeTab = this.tabs[0].id;
        }

        let html = `<div class="tabs-container" id="${this.id}" data-orientation="${this.orientation}">`;

        html += `<div class="tabs-list tabs-list-${this.variant}">`;

        for (const tab of this.tabs) {
            const isActive = tab.id === this.activeTab;
            
            html += `<button type="button" 
                class="tabs-trigger ${isActive ? 'tabs-active' : ''}"
                data-tab="${tab.id}"
                ${tab.disabled ? 'disabled' : ''}
            >
                ${tab.icon ? `<span class="tabs-icon">${tab.icon}</span>` : ''}
                <span class="tabs-label">${tab.label}</span>
                ${tab.badge ? `<span class="tabs-badge">${tab.badge}</span>` : ''}
            </button>`;
        }

        html += `</div>`;

        html += `<div class="tabs-panels">`;

        for (const tab of this.tabs) {
            const isActive = tab.id === this.activeTab;
            
            html += `<div 
                class="tabs-panel ${isActive ? 'tabs-panel-active' : ''}"
                data-panel="${tab.id}"
                ${!isActive && this.lazy ? 'hidden' : ''}
            >
                ${typeof tab.content === 'function' ? tab.content() : tab.content}
            </div>`;
        }

        html += `</div>`;
        html += `</div>`;

        return html;
    }

    setActiveTab(tabId) {
        const tab = this.tabs.find(t => t.id === tabId);
        if (!tab || tab.disabled) return;

        this.activeTab = tabId;

        if (this.onChange) {
            this.onChange(tabId, tab);
        }
    }

    addTab(tab) {
        this.tabs.push(tab);
    }

    removeTab(tabId) {
        this.tabs = this.tabs.filter(t => t.id !== tabId);
        
        if (this.activeTab === tabId && this.tabs.length > 0) {
            this.activeTab = this.tabs[0].id;
        }
    }
}

export class Dropdown {
    constructor(options = {}) {
        this.id = options.id || 'dropdown_' + Date.now();
        this.trigger = options.trigger || 'Click';
        this.menu = options.menu || [];
        this.align = options.align || 'left';
        this.hoverable = options.hoverable ?? false;
        this.disabled = options.disabled || false;
        this.visible = false;
        this.onChange = options.onChange || null;
    }

    render() {
        let html = `<div class="dropdown" id="${this.id}" data-align="${this.align}">`;

        html += `<button type="button" class="dropdown-trigger" data-dropdown-trigger="${this.id}">
            ${this.trigger}
            <span class="dropdown-arrow">▼</span>
        </button>`;

        html += `<div class="dropdown-menu ${this.visible ? 'dropdown-visible' : ''}">`;

        for (const item of this.menu) {
            if (item.divider) {
                html += `<div class="dropdown-divider"></div>`;
            } else {
                const disabledClass = item.disabled ? 'dropdown-item-disabled' : '';
                const iconClass = item.icon ? 'dropdown-item-with-icon' : '';
                
                html += `<button type="button" 
                    class="dropdown-item ${disabledClass} ${iconClass}"
                    data-value="${item.value || item.label}"
                    ${item.disabled ? 'disabled' : ''}
                >
                    ${item.icon ? `<span class="dropdown-item-icon">${item.icon}</span>` : ''}
                    <span class="dropdown-item-label">${item.label}</span>
                    ${item.badge ? `<span class="dropdown-item-badge">${item.badge}</span>` : ''}
                </button>`;
            }
        }

        html += `</div>`;
        html += `</div>`;

        return html;
    }

    show() {
        this.visible = true;
    }

    hide() {
        this.visible = false;
    }

    toggle() {
        this.visible = !this.visible;
    }

    addItem(item) {
        this.menu.push(item);
    }

    removeItem(value) {
        this.menu = this.menu.filter(item => item.value !== value);
    }
}

export class Toast {
    constructor(options = {}) {
        this.id = options.id || 'toast_' + Date.now();
        this.type = options.type || 'info';
        this.title = options.title || '';
        this.message = options.message || '';
        this.duration = options.duration || 5000;
        this.dismissible = options.dismissible ?? true;
        this.progress = options.progress ?? false;
        this.icon = options.icon || null;
        this.actions = options.actions || [];
        this.visible = true;
        this.createdAt = Date.now();
    }

    render() {
        const icons = {
            success: '✓',
            error: '✕',
            warning: '⚠',
            info: 'ℹ'
        };

        let html = `<div id="${this.id}" class="toast toast-${this.type}">`;

        if (this.icon || this.type) {
            html += `<span class="toast-icon">
                ${this.icon || icons[this.type] || icons.info}
            </span>`;
        }

        html += `<div class="toast-content">`;
        
        if (this.title) {
            html += `<div class="toast-title">${this.title}</div>`;
        }
        
        html += `<div class="toast-message">${this.message}</div>`;
        
        if (this.actions.length > 0) {
            html += `<div class="toast-actions">`;
            for (const action of this.actions) {
                html += `<button type="button" class="toast-action-btn" data-action="${action.action}">
                    ${action.label}
                </button>`;
            }
            html += `</div>`;
        }
        
        html += `</div>`;

        if (this.dismissible) {
            html += `<button type="button" class="toast-close-btn" data-dismiss="${this.id}">×</button>`;
        }

        if (this.progress) {
            html += `<div class="toast-progress"></div>`;
        }

        html += `</div>`;

        return html;
    }

    dismiss() {
        this.visible = false;
    }
}

export class ToastManager {
    constructor() {
        this.toasts = new Map();
        this.position = 'top-right';
        this.maxToasts = 5;
    }

    show(options) {
        const toast = new Toast(options);
        this.toasts.set(toast.id, toast);

        if (this.toasts.size > this.maxToasts) {
            const oldest = Array.from(this.toasts.values())[0];
            this.dismiss(oldest.id);
        }

        if (toast.duration > 0) {
            setTimeout(() => {
                this.dismiss(toast.id);
            }, toast.duration);
        }

        return toast.id;
    }

    success(message, options = {}) {
        return this.show({ ...options, type: 'success', message });
    }

    error(message, options = {}) {
        return this.show({ ...options, type: 'error', message });
    }

    warning(message, options = {}) {
        return this.show({ ...options, type: 'warning', message });
    }

    info(message, options = {}) {
        return this.show({ ...options, type: 'info', message });
    }

    dismiss(id) {
        this.toasts.delete(id);
    }

    dismissAll() {
        this.toasts.clear();
    }

    getToasts() {
        return Array.from(this.toasts.values());
    }
}

export default {
    Button,
    Input,
    Select,
    Modal,
    Card,
    Table,
    Tabs,
    Dropdown,
    Toast,
    ToastManager
};
