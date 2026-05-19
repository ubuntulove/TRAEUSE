export class ConsoleCommand {
    constructor(name, callback, description = '', parameters = []) {
        this.name = name;
        this.callback = callback;
        this.description = description;
        this.parameters = parameters;
        this.permissionLevel = 0;
        this.isHidden = false;
        this.isCheat = false;
        this.isClientOnly = false;
        this.isServerOnly = false;
    }

    execute(args) {
        if (this.parameters.length > 0) {
            const validatedArgs = this.validateArgs(args);
            if (validatedArgs === null) return false;
            return this.callback(validatedArgs);
        }
        return this.callback(args);
    }

    validateArgs(args) {
        if (args.length < this.parameters.filter(p => !p.optional).length) {
            return null;
        }

        const validated = {};
        for (let i = 0; i < this.parameters.length; i++) {
            const param = this.parameters[i];
            const arg = args[i];

            if (arg === undefined && param.optional) {
                validated[param.name] = param.defaultValue;
                continue;
            }

            if (arg === undefined && !param.optional) {
                return null;
            }

            const value = this.parseValue(arg, param.type);
            if (value === null) {
                return null;
            }

            validated[param.name] = value;
        }

        return validated;
    }

    parseValue(value, type) {
        switch (type) {
            case 'string':
                return String(value);
            case 'number':
            case 'float':
                const num = parseFloat(value);
                return isNaN(num) ? null : num;
            case 'int':
                const int = parseInt(value, 10);
                return isNaN(int) ? null : int;
            case 'bool':
                if (value === 'true' || value === '1') return true;
                if (value === 'false' || value === '0') return false;
                return null;
            case 'player':
                return value;
            case 'team':
                return value;
            default:
                return value;
        }
    }
}

export class ConsoleVariable {
    constructor(name, defaultValue, description = '', flags = 0) {
        this.name = name;
        this.defaultValue = defaultValue;
        this.currentValue = defaultValue;
        this.description = description;
        this.flags = flags;
        this.type = this.inferType(defaultValue);
        this.onChangeCallbacks = [];
        this.minValue = null;
        this.maxValue = null;
        this.restartRequired = false;
        this.cheat = false;
    }

    inferType(value) {
        if (typeof value === 'boolean') return 'bool';
        if (typeof value === 'number') {
            return Number.isInteger(value) ? 'int' : 'float';
        }
        return 'string';
    }

    set(value) {
        let newValue;

        if (this.type === 'int' || this.type === 'float') {
            newValue = parseFloat(value);
            if (isNaN(newValue)) return false;
            if (this.minValue !== null && newValue < this.minValue) return false;
            if (this.maxValue !== null && newValue > this.maxValue) return false;
        } else if (this.type === 'bool') {
            if (value === 'true' || value === '1') newValue = true;
            else if (value === 'false' || value === '0') newValue = false;
            else return false;
        } else {
            newValue = value;
        }

        const oldValue = this.currentValue;
        this.currentValue = newValue;

        for (const callback of this.onChangeCallbacks) {
            callback(newValue, oldValue);
        }

        return true;
    }

    get() {
        return this.currentValue;
    }

    reset() {
        this.set(this.defaultValue);
    }

    onChange(callback) {
        this.onChangeCallbacks.push(callback);
    }

    setRange(min, max) {
        this.minValue = min;
        this.maxValue = max;
    }

    setFlags(flags) {
        this.flags = flags;
    }

    serialize() {
        return {
            name: this.name,
            value: this.currentValue,
            defaultValue: this.defaultValue,
            type: this.type,
            description: this.description,
            flags: this.flags
        };
    }
}

export class Console {
    constructor() {
        this.commands = new Map();
        this.variables = new Map();
        this.history = [];
        this.maxHistory = 100;
        this.logBuffer = [];
        this.maxLogBuffer = 1000;
        this.consoleBuffer = [];
        this.maxConsoleBuffer = 500;
        this.isOpen = false;
        this.filters = {
            info: true,
            warning: true,
            error: true,
            debug: true
        };
        this.registerDefaultCommands();
        this.registerDefaultVariables();
    }

    registerDefaultCommands() {
        this.registerCommand(new ConsoleCommand(
            'help',
            (args) => this.showHelp(args),
            'Show help information',
            [{ name: 'command', type: 'string', optional: true }]
        ));

        this.registerCommand(new ConsoleCommand(
            'echo',
            (args) => {
                this.log(args.message);
                return true;
            },
            'Print a message to the console',
            [{ name: 'message', type: 'string' }]
        ));

        this.registerCommand(new ConsoleCommand(
            'clear',
            () => {
                this.consoleBuffer = [];
                return true;
            },
            'Clear the console'
        ));

        this.registerCommand(new ConsoleCommand(
            'list',
            () => {
                this.log('Registered commands:');
                for (const [name, cmd] of this.commands) {
                    if (!cmd.isHidden) {
                        this.log(`  ${name}: ${cmd.description}`);
                    }
                }
                return true;
            },
            'List all registered commands'
        ));

        this.registerCommand(new ConsoleCommand(
            'listvars',
            () => {
                this.log('Console variables:');
                for (const [name, variable] of this.variables) {
                    this.log(`  ${name} = ${variable.get()} (default: ${variable.defaultValue})`);
                }
                return true;
            },
            'List all console variables'
        ));

        this.registerCommand(new ConsoleCommand(
            'exec',
            (args) => {
                return this.executeFile(args.filename);
            },
            'Execute commands from a file',
            [{ name: 'filename', type: 'string' }]
        ));

        this.registerCommand(new ConsoleCommand(
            'find',
            (args) => {
                const query = args.query.toLowerCase();
                const results = [];
                
                for (const [name, cmd] of this.commands) {
                    if (name.toLowerCase().includes(query) || cmd.description.toLowerCase().includes(query)) {
                        results.push(`cmd: ${name}`);
                    }
                }
                
                for (const [name, variable] of this.variables) {
                    if (name.toLowerCase().includes(query)) {
                        results.push(`var: ${name} = ${variable.get()}`);
                    }
                }
                
                if (results.length > 0) {
                    this.log('Search results:');
                    results.forEach(r => this.log(`  ${r}`));
                } else {
                    this.log('No results found');
                }
                return true;
            },
            'Find commands and variables',
            [{ name: 'query', type: 'string' }]
        ));

        this.registerCommand(new ConsoleCommand(
            'history',
            () => {
                this.log('Command history:');
                this.history.slice(-20).forEach((cmd, i) => {
                    this.log(`  ${i + 1}: ${cmd}`);
                });
                return true;
            },
            'Show command history'
        ));
    }

    registerDefaultVariables() {
        this.registerVariable(new ConsoleVariable(
            'cl_showfps', 1,
            'Show FPS counter (0=off, 1=top-left, 2=top-right)'
        ).setRange(0, 2));

        this.registerVariable(new ConsoleVariable(
            'fps_max', 300,
            'Maximum frames per second'
        ).setRange(30, 500));

        this.registerVariable(new ConsoleVariable(
            'sensitivity', 2.5,
            'Mouse sensitivity'
        ).setRange(0.1, 10));

        this.registerVariable(new ConsoleVariable(
            'volume', 0.5,
            'Master volume'
        ).setRange(0, 1));

        this.registerVariable(new ConsoleVariable(
            'voice_enable', 1,
            'Enable voice chat'
        ));

        this.registerVariable(new ConsoleVariable(
            'crosshair', 1,
            'Show crosshair'
        ));

        this.registerVariable(new ConsoleVariable(
            'cl_radar_rotate', 1,
            'Rotate radar with player'
        ));

        this.registerVariable(new ConsoleVariable(
            'net_graph', 0,
            'Show network graph (0=off, 1-3=levels)'
        ).setRange(0, 3));

        this.registerVariable(new ConsoleVariable(
            'sv_cheats', 0,
            'Allow cheat commands'
        ));

        this.registerVariable(new ConsoleVariable(
            'mp_roundtime', 115,
            'Round time in seconds'
        ));

        this.registerVariable(new ConsoleVariable(
            'mp_maxrounds', 30,
            'Maximum rounds before map change'
        ));

        this.registerVariable(new ConsoleVariable(
            'mp_freezetime', 15,
            'Freeze time at round start'
        ));

        this.registerVariable(new ConsoleVariable(
            'mp_startmoney', 800,
            'Starting money'
        ).setRange(0, 16000));

        this.registerVariable(new ConsoleVariable(
            'mp_buy_anywhere', 0,
            'Allow buying anywhere on map'
        ));

        this.registerVariable(new ConsoleVariable(
            'mp_buytime', 20,
            'Buy time in seconds'
        ).setRange(0, 300));

        this.registerVariable(new ConsoleVariable(
            'friendlyfire', 0,
            'Enable friendly fire'
        ));

        this.registerVariable(new ConsoleVariable(
            'god', 0,
            'God mode (invincibility)'
        ));

        this.registerVariable(new ConsoleVariable(
            'noclip', 0,
            'No clip mode (fly through walls)'
        ));

        this.registerVariable(new ConsoleVariable(
            'give', 'weapon_ak47',
            'Give weapon by name'
        ));

        this.registerVariable(new ConsoleVariable(
            'maps', '',
            'Current map'
        ));

        this.registerVariable(new ConsoleVariable(
            'hostname', 'CS2 Server',
            'Server hostname'
        ));

        this.registerVariable(new ConsoleVariable(
            'maxplayers', 10,
            'Maximum players'
        ).setRange(2, 64));

        this.registerVariable(new ConsoleVariable(
            'r_drawviewmodels', 1,
            'Draw view models'
        ));

        this.registerVariable(new ConsoleVariable(
            'r_fullbright', 0,
            'Full brightness lighting'
        ));

        this.registerVariable(new ConsoleVariable(
            'mat_queue_mode', 2,
            'Material queue mode'
        ));

        this.registerVariable(new ConsoleVariable(
            'cl_interp', 0.031,
            'Interpolation amount'
        ).setRange(0, 1));

        this.registerVariable(new ConsoleVariable(
            'cl_cmdrate', 128,
            'Command rate'
        ).setRange(20, 128));

        this.registerVariable(new ConsoleVariable(
            'cl_updaterate', 128,
            'Update rate'
        ).setRange(20, 128));

        this.registerVariable(new ConsoleVariable(
            'hud_scaling', 0.85,
            'HUD scale'
        ).setRange(0.5, 1));

        this.registerVariable(new ConsoleVariable(
            'cl_showloadout', 1,
            'Show loadout in HUD'
        ));

        this.registerVariable(new ConsoleVariable(
            'weapon_debug_spread_show', 0,
            'Show weapon spread'
        ).setRange(0, 2));
    }

    registerCommand(command) {
        this.commands.set(command.name, command);
    }

    unregisterCommand(name) {
        return this.commands.delete(name);
    }

    registerVariable(variable) {
        this.variables.set(variable.name, variable);
    }

    unregisterVariable(name) {
        return this.variables.delete(name);
    }

    getCommand(name) {
        return this.commands.get(name);
    }

    getVariable(name) {
        return this.variables.get(name);
    }

    execute(input) {
        input = input.trim();
        if (!input) return null;

        if (input.startsWith('@')) {
            this.history.push(input.substring(1));
        } else {
            this.history.push(input);
        }

        if (this.history.length > this.maxHistory) {
            this.history.shift();
        }

        if (input.startsWith('.')) {
            const varName = input.substring(1);
            const variable = this.variables.get(varName);
            if (variable) {
                this.log(`${varName} = ${variable.get()}`);
                return true;
            }
        }

        if (input.startsWith('$')) {
            const parts = input.substring(1).split(' ');
            const varName = parts[0];
            const value = parts.slice(1).join(' ');

            const variable = this.variables.get(varName);
            if (variable) {
                if (value) {
                    return variable.set(value);
                } else {
                    this.log(`${varName} = ${variable.get()}`);
                    return true;
                }
            }
        }

        const parts = input.split(' ');
        const commandName = parts[0];
        const args = parts.slice(1);

        const command = this.commands.get(commandName);
        if (command) {
            try {
                return command.execute(args);
            } catch (e) {
                this.error(`Command error: ${e.message}`);
                return false;
            }
        }

        this.error(`Unknown command: ${commandName}`);
        return false;
    }

    executeFile(filename) {
        this.log(`Executing file: ${filename}`);
        return true;
    }

    showHelp(args) {
        if (args.command) {
            const command = this.commands.get(args.command);
            if (command) {
                this.log(`Command: ${command.name}`);
                this.log(`Description: ${command.description}`);
                if (command.parameters.length > 0) {
                    this.log('Parameters:');
                    for (const param of command.parameters) {
                        const optional = param.optional ? ' (optional)' : '';
                        this.log(`  ${param.name}: ${param.type}${optional}`);
                    }
                }
                return true;
            }

            const variable = this.variables.get(args.command);
            if (variable) {
                this.log(`Variable: ${variable.name}`);
                this.log(`Default value: ${variable.defaultValue}`);
                this.log(`Current value: ${variable.get()}`);
                this.log(`Description: ${variable.description}`);
                return true;
            }

            this.error(`Unknown command or variable: ${args.command}`);
            return false;
        }

        this.log('Available commands:');
        for (const [name, cmd] of this.commands) {
            if (!cmd.isHidden) {
                this.log(`  ${name}: ${cmd.description}`);
            }
        }
        return true;
    }

    log(message, type = 'info') {
        const entry = {
            message: String(message),
            type,
            timestamp: Date.now()
        };

        this.logBuffer.push(entry);
        this.consoleBuffer.push(entry);

        if (this.logBuffer.length > this.maxLogBuffer) {
            this.logBuffer.shift();
        }
        if (this.consoleBuffer.length > this.maxConsoleBuffer) {
            this.consoleBuffer.shift();
        }
    }

    info(message) {
        this.log(message, 'info');
    }

    warn(message) {
        this.log(message, 'warning');
    }

    error(message) {
        this.log(message, 'error');
    }

    debug(message) {
        this.log(message, 'debug');
    }

    clear() {
        this.consoleBuffer = [];
        this.logBuffer = [];
    }

    getBuffer(filter = null) {
        if (filter) {
            return this.consoleBuffer.filter(entry => entry.type === filter);
        }
        return this.consoleBuffer;
    }

    setFilter(type, enabled) {
        this.filters[type] = enabled;
    }

    open() {
        this.isOpen = true;
    }

    close() {
        this.isOpen = false;
    }

    toggle() {
        this.isOpen = !this.isOpen;
    }

    getAllCommands() {
        return Array.from(this.commands.values());
    }

    getAllVariables() {
        return Array.from(this.variables.values());
    }

    serialize() {
        return {
            commandCount: this.commands.size,
            variableCount: this.variables.size,
            historyLength: this.history.length,
            bufferLength: this.consoleBuffer.length
        };
    }
}

export class ConsoleManager {
    constructor() {
        this.consoles = new Map();
        this.defaultConsole = new Console();
        this.bindings = new Map();
        this.keyBindings = new Map();
        this.registerDefaultBindings();
    }

    registerDefaultBindings() {
        this.bindKey('`', 'toggleconsole');
        this.bindKey('~', 'toggleconsole');
        this.bindKey('F1', 'toggleconsole');
        this.bindKey('Escape', 'cancelselect');
        this.bindKey('Tab', '+showscores');
        this.bindKey('M', 'togglemenu');
        this.bindKey('Y', 'messagemode');
        this.bindKey('U', 'messagemode2');
        this.bindKey('T', 'messagemode2');
        this.bindKey('Z', 'slot1');
        this.bindKey('X', 'slot2');
        this.bindKey('C', 'slot3');
        this.bindKey('V', 'slot4');
        this.bindKey('B', 'slot5');
        this.bindKey('G', 'slot6');
        this.bindKey('H', 'slot7');
        this.bindKey('J', 'slot8');
        this.bindKey('K', 'slot9');
        this.bindKey('Space', '+jump');
        this.bindKey('Shift_L', '+speed');
        this.bindKey('Shift_R', '+speed');
        this.bindKey('Ctrl_L', '+duck');
        this.bindKey('Ctrl_R', '+duck');
        this.bindKey('Mouse_Left', '+attack');
        this.bindKey('Mouse_Right', '+attack2');
        this.bindKey('Mouse_WheelUp', '+lookspin');
        this.bindKey('Mouse_WheelDown', '-lookspin');
        this.bindKey('MWheelUp', 'invprev');
        this.bindKey('MWheelDown', 'invnext');
        this.bindKey('1', 'slot1');
        this.bindKey('2', 'slot2');
        this.bindKey('3', 'slot3');
        this.bindKey('4', 'slot4');
        this.bindKey('5', 'slot5');
        this.bindKey('6', 'slot6');
        this.bindKey('7', 'slot7');
        this.bindKey('8', 'slot8');
        this.bindKey('9', 'slot9');
        this.bindKey('0', 'slot10');
    }

    getConsole(playerId = null) {
        if (playerId === null) {
            return this.defaultConsole;
        }
        if (!this.consoles.has(playerId)) {
            this.consoles.set(playerId, new Console());
        }
        return this.consoles.get(playerId);
    }

    execute(command, playerId = null) {
        const console = this.getConsole(playerId);
        return console.execute(command);
    }

    registerGlobalCommand(command) {
        this.defaultConsole.registerCommand(command);
    }

    registerGlobalVariable(variable) {
        this.defaultConsole.registerVariable(variable);
    }

    bindKey(key, command) {
        this.keyBindings.set(key, command);
    }

    unbindKey(key) {
        return this.keyBindings.delete(key);
    }

    getBinding(key) {
        return this.keyBindings.get(key);
    }

    handleKeyPress(key) {
        const command = this.keyBindings.get(key);
        if (command) {
            if (command.startsWith('+')) {
                this.execute(command.substring(1) + ' 1');
            } else if (command.startsWith('-')) {
                this.execute(command.substring(1) + ' 0');
            } else {
                this.execute(command);
            }
            return true;
        }
        return false;
    }

    loadBindings(playerId, bindings) {
        const console = this.getConsole(playerId);
        for (const [key, command] of Object.entries(bindings)) {
            console.bindKey(key, command);
        }
    }

    saveBindings(playerId) {
        const console = this.getConsole(playerId);
        return Object.fromEntries(console.keyBindings);
    }

    serialize() {
        return {
            globalCommands: this.defaultConsole.commands.size,
            globalVariables: this.defaultConsole.variables.size,
            playerConsoles: this.consoles.size,
            keyBindings: this.keyBindings.size
        };
    }
}

export class ReportSystem {
    constructor() {
        this.reports = new Map();
        this.reportReasons = [
            'cheating',
            'griefing',
            'toxicity',
            'exploiting',
            'spam',
            'harassment',
            'mic_spam',
            'abusive_name',
            'other'
        ];
        this.reportQueue = [];
        this.autoMuteThreshold = 5;
        this.autoKickThreshold = 10;
        this.banThreshold = 20;
    }

    createReport(reporterId, targetId, reason, details = '', evidence = []) {
        const reportId = `report_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
        
        const report = {
            id: reportId,
            reporterId,
            targetId,
            reason,
            details,
            evidence,
            status: 'pending',
            priority: this.calculatePriority(reason),
            createdAt: Date.now(),
            reviewedAt: null,
            reviewedBy: null,
            verdict: null,
            action: null
        };

        this.reports.set(reportId, report);
        this.updateTargetReportCount(targetId);
        this.processReport(report);

        return report;
    }

    calculatePriority(reason) {
        const priorities = {
            'cheating': 10,
            'exploiting': 9,
            'griefing': 7,
            'harassment': 6,
            'toxicity': 5,
            'abusive_name': 4,
            'mic_spam': 3,
            'spam': 2,
            'other': 1
        };
        return priorities[reason] || 1;
    }

    updateTargetReportCount(targetId) {
        let count = 0;
        for (const report of this.reports.values()) {
            if (report.targetId === targetId && report.status === 'pending') {
                count++;
            }
        }
        return count;
    }

    processReport(report) {
        const reportCount = this.updateTargetReportCount(report.targetId);

        if (reportCount >= this.banThreshold) {
            report.action = 'ban';
            report.status = 'actioned';
        } else if (reportCount >= this.autoKickThreshold) {
            report.action = 'kick';
            report.status = 'actioned';
        } else if (reportCount >= this.autoMuteThreshold) {
            report.action = 'mute';
            report.status = 'actioned';
        }

        if (report.priority >= 8) {
            this.reportQueue.push(report);
            this.reportQueue.sort((a, b) => b.priority - a.priority);
        }
    }

    reviewReport(reportId, investigatorId, verdict, action = null, notes = '') {
        const report = this.reports.get(reportId);
        if (!report) return false;

        report.status = 'reviewed';
        report.reviewedAt = Date.now();
        report.reviewedBy = investigatorId;
        report.verdict = verdict;
        if (action) {
            report.action = action;
            report.status = 'actioned';
        }
        report.notes = notes;

        return true;
    }

    getReport(reportId) {
        return this.reports.get(reportId);
    }

    getReportsByTarget(targetId) {
        return Array.from(this.reports.values())
            .filter(r => r.targetId === targetId);
    }

    getReportsByReporter(reporterId) {
        return Array.from(this.reports.values())
            .filter(r => r.reporterId === reporterId);
    }

    getPendingReports() {
        return Array.from(this.reports.values())
            .filter(r => r.status === 'pending')
            .sort((a, b) => b.priority - a.priority);
    }

    getReviewedReports(investigatorId = null) {
        let reports = Array.from(this.reports.values())
            .filter(r => r.status === 'reviewed' || r.status === 'actioned');

        if (investigatorId) {
            reports = reports.filter(r => r.reviewedBy === investigatorId);
        }

        return reports;
    }

    getNextReport() {
        return this.reportQueue.shift() || null;
    }

    getReportStats(targetId) {
        const reports = this.getReportsByTarget(targetId);
        const reasons = {};

        for (const report of reports) {
            reasons[report.reason] = (reasons[report.reason] || 0) + 1;
        }

        return {
            totalReports: reports.length,
            pendingReports: reports.filter(r => r.status === 'pending').length,
            actionedReports: reports.filter(r => r.status === 'actioned').length,
            reasons,
            reportRate: this.calculateReportRate(reports)
        };
    }

    calculateReportRate(reports) {
        if (reports.length === 0) return 0;

        const recentReports = reports.filter(r => 
            Date.now() - r.createdAt < 604800000
        );

        return recentReports.length;
    }

    dismissReports(targetId, reason = '') {
        let dismissed = 0;
        for (const report of this.reports.values()) {
            if (report.targetId === targetId && report.status === 'pending') {
                report.status = 'dismissed';
                dismissed++;
            }
        }
        return dismissed;
    }

    serialize() {
        return {
            totalReports: this.reports.size,
            pendingReports: this.getPendingReports().length,
            reportReasons: this.reportReasons
        };
    }
}

export class PartySystem {
    constructor() {
        this.parties = new Map();
        this.partyInvites = new Map();
        this.maxPartySize = 5;
        this.partyChat = new Map();
    }

    createParty(leaderId, name = null) {
        const partyId = `party_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

        const party = {
            id: partyId,
            name: name || `Party ${leaderId}`,
            leaderId,
            members: [leaderId],
            invited: [],
            maxSize: this.maxPartySize,
            gameMode: 'competitive',
            region: 'any',
            privacy: 'public',
            createdAt: Date.now(),
            status: 'idle',
            matchId: null,
            settings: {
                allowVoiceChat: true,
                allowTextChat: true,
                allowInvites: true
            }
        };

        this.parties.set(partyId, party);
        this.partyChat.set(partyId, []);

        return party;
    }

    getParty(partyId) {
        return this.parties.get(partyId);
    }

    getPlayerParty(playerId) {
        for (const party of this.parties.values()) {
            if (party.members.includes(playerId)) {
                return party;
            }
        }
        return null;
    }

    isInParty(playerId) {
        return this.getPlayerParty(playerId) !== null;
    }

    joinParty(partyId, playerId) {
        const party = this.parties.get(partyId);
        if (!party) return { success: false, error: 'Party not found' };

        if (party.members.includes(playerId)) {
            return { success: false, error: 'Already in party' };
        }

        if (party.members.length >= party.maxSize) {
            return { success: false, error: 'Party is full' };
        }

        if (party.invited.length > 0 && !party.invited.includes(playerId)) {
            return { success: false, error: 'Not invited' };
        }

        if (party.privacy === 'private' && !party.invited.includes(playerId)) {
            return { success: false, error: 'Party is private' };
        }

        party.members.push(playerId);
        party.invited = party.invited.filter(id => id !== playerId);

        return { success: true, party };
    }

    leaveParty(playerId) {
        const party = this.getPlayerParty(playerId);
        if (!party) return { success: false, error: 'Not in a party' };

        party.members = party.members.filter(id => id !== playerId);

        if (party.members.length === 0) {
            this.parties.delete(party.id);
            this.partyChat.delete(party.id);
        } else if (party.leaderId === playerId) {
            party.leaderId = party.members[0];
        }

        return { success: true };
    }

    kickPlayer(partyId, kickerId, targetId) {
        const party = this.parties.get(partyId);
        if (!party) return { success: false, error: 'Party not found' };

        if (party.leaderId !== kickerId) {
            return { success: false, error: 'Not party leader' };
        }

        if (targetId === kickerId) {
            return { success: false, error: 'Cannot kick yourself' };
        }

        party.members = party.members.filter(id => id !== targetId);

        return { success: true };
    }

    invitePlayer(partyId, inviterId, targetId) {
        const party = this.parties.get(partyId);
        if (!party) return { success: false, error: 'Party not found' };

        if (!party.members.includes(inviterId) && party.leaderId !== inviterId) {
            return { success: false, error: 'Not in party' };
        }

        if (party.members.includes(targetId)) {
            return { success: false, error: 'Already in party' };
        }

        if (party.invited.includes(targetId)) {
            return { success: false, error: 'Already invited' };
        }

        if (party.members.length + party.invited.length >= party.maxSize) {
            return { success: false, error: 'Cannot invite more players' };
        }

        party.invited.push(targetId);

        const inviteId = `invite_${partyId}_${targetId}`;
        const invite = {
            id: inviteId,
            partyId,
            partyName: party.name,
            inviterId,
            targetId,
            createdAt: Date.now(),
            expiresAt: Date.now() + 60000
        };

        this.partyInvites.set(inviteId, invite);

        return { success: true, invite };
    }

    acceptInvite(inviteId, playerId) {
        const invite = this.partyInvites.get(inviteId);
        if (!invite) return { success: false, error: 'Invite not found' };

        if (invite.targetId !== playerId) {
            return { success: false, error: 'Not your invite' };
        }

        if (Date.now() > invite.expiresAt) {
            this.partyInvites.delete(inviteId);
            return { success: false, error: 'Invite expired' };
        }

        this.partyInvites.delete(inviteId);
        return this.joinParty(invite.partyId, playerId);
    }

    declineInvite(inviteId, playerId) {
        const invite = this.partyInvites.get(inviteId);
        if (!invite) return false;

        if (invite.targetId !== playerId) return false;

        this.partyInvites.delete(inviteId);
        return true;
    }

    getPendingInvites(playerId) {
        const invites = [];
        for (const invite of this.partyInvites.values()) {
            if (invite.targetId === playerId && Date.now() < invite.expiresAt) {
                invites.push(invite);
            }
        }
        return invites;
    }

    setLeader(partyId, setterId, newLeaderId) {
        const party = this.parties.get(partyId);
        if (!party) return false;

        if (party.leaderId !== setterId) return false;

        if (!party.members.includes(newLeaderId)) return false;

        party.leaderId = newLeaderId;
        return true;
    }

    setPrivacy(partyId, setterId, privacy) {
        const party = this.parties.get(partyId);
        if (!party) return false;

        if (party.leaderId !== setterId) return false;

        party.privacy = privacy;
        return true;
    }

    setGameMode(partyId, setterId, gameMode) {
        const party = this.parties.get(partyId);
        if (!party) return false;

        if (!party.members.includes(setterId)) return false;

        party.gameMode = gameMode;
        return true;
    }

    setRegion(partyId, setterId, region) {
        const party = this.parties.get(partyId);
        if (!party) return false;

        if (!party.members.includes(setterId)) return false;

        party.region = region;
        return true;
    }

    sendChatMessage(partyId, senderId, message) {
        const party = this.parties.get(partyId);
        if (!party) return false;

        if (!party.members.includes(senderId)) return false;

        const chat = this.partyChat.get(partyId);
        if (!chat) return false;

        chat.push({
            senderId,
            message,
            timestamp: Date.now()
        });

        if (chat.length > 100) {
            chat.shift();
        }

        return true;
    }

    getChatMessages(partyId, since = 0) {
        const chat = this.partyChat.get(partyId);
        if (!chat) return [];

        return chat.filter(msg => msg.timestamp > since);
    }

    getAllParties() {
        return Array.from(this.parties.values());
    }

    getPublicParties() {
        return Array.from(this.parties.values())
            .filter(p => p.privacy === 'public');
    }

    serialize() {
        return {
            totalParties: this.parties.size,
            activeInvites: this.partyInvites.size
        };
    }
}

export default ConsoleManager;
