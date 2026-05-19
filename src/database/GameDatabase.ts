export interface GameDatabase {
  id: string;
  name: string;
  version: string;
  schema: DatabaseSchema;
  collections: Map<string, Collection>;
  indexes: Map<string, Index>;
  transactionLog: TransactionLogEntry[];
  statistics: DatabaseStatistics;
  settings: DatabaseSettings;
}

export interface DatabaseSchema {
  version: number;
  collections: CollectionSchema[];
  validation: ValidationRule[];
  migrations: Migration[];
}

export interface CollectionSchema {
  name: string;
  fields: FieldSchema[];
  indexes: IndexSchema[];
  options: CollectionOptions;
}

export interface FieldSchema {
  name: string;
  type: FieldType;
  required: boolean;
  default?: unknown;
  validation?: ValidationRule;
  reference?: string;
  array: boolean;
  enum?: string[];
}

export type FieldType =
  | 'string'
  | 'number'
  | 'boolean'
  | 'date'
  | 'object'
  | 'array'
  | 'reference'
  | 'uuid'
  | 'email'
  | 'url'
  | 'regex';

export interface IndexSchema {
  fields: string[];
  type: IndexType;
  unique: boolean;
  sparse: boolean;
}

export type IndexType =
  | 'single'
  | 'compound'
  | 'text'
  | '2dsphere'
  | '2d'
  | 'hashed';

export interface CollectionOptions {
  capped: boolean;
  maxSize?: number;
  maxDocuments?: number;
  validator?: ValidationRule;
  validationLevel: ValidationLevel;
  validationAction: ValidationAction;
}

export type ValidationLevel =
  | 'off'
  | 'strict'
  | 'moderate';

export type ValidationAction =
  | 'error'
  | 'warn';

export interface Collection {
  name: string;
  documents: Map<string, Document>;
  indexes: Map<string, Index>;
  options: CollectionOptions;
}

export interface Document {
  _id: string;
  _createdAt: number;
  _updatedAt: number;
  _version: number;
  _deleted: boolean;
  _checksum?: string;
  data: Record<string, unknown>;
}

export interface Index {
  id: string;
  name: string;
  fields: string[];
  type: IndexType;
  unique: boolean;
  sparse: boolean;
  entries: Map<string, string[]>;
  size: number;
}

export interface TransactionLogEntry {
  id: string;
  type: TransactionType;
  collection: string;
  documentId: string;
  timestamp: number;
  operation: Operation;
  before?: Record<string, unknown>;
  after?: Record<string, unknown>;
  userId?: string;
  sessionId?: string;
  rollback?: boolean;
}

export type TransactionType =
  | 'insert'
  | 'update'
  | 'delete'
  | 'replace'
  | 'bulk';

export interface Operation {
  type: 'insert' | 'update' | 'delete' | 'find' | 'aggregate' | 'count' | 'distinct';
  query?: Record<string, unknown>;
  update?: Record<string, unknown>;
  pipeline?: AggregationPipeline;
  options?: OperationOptions;
}

export interface OperationOptions {
  upsert?: boolean;
  multi?: boolean;
  writeConcern?: WriteConcern;
  ordered?: boolean;
  bypassDocumentValidation?: boolean;
}

export interface WriteConcern {
  w?: number | 'majority' | string;
  j?: boolean;
  wtimeout?: number;
}

export interface AggregationPipeline {
  stages: AggregationStage[];
}

export interface AggregationStage {
  $match?: Record<string, unknown>;
  $group?: Record<string, unknown>;
  $sort?: Record<string, 1 | -1>;
  $limit?: number;
  $skip?: number;
  $project?: Record<string, unknown>;
  $unwind?: string | { path: string; preserveNullAndEmptyArrays?: boolean };
  $lookup?: LookupOptions;
  $addFields?: Record<string, unknown>;
  $count?: string;
  $facet?: Record<string, AggregationPipeline>;
  $bucket?: BucketOptions;
  $sortByCount?: string;
  $geoNear?: GeoNearOptions;
  $sample?: { size: number };
  $unionWith?: { coll: string; pipeline?: AggregationPipeline };
}

export interface LookupOptions {
  from: string;
  localField: string;
  foreignField: string;
  as: string;
  pipeline?: AggregationPipeline;
  let?: Record<string, unknown>;
}

export interface BucketOptions {
  groupBy: string;
  boundaries: unknown[];
  default?: string;
  output?: Record<string, unknown>;
}

export interface GeoNearOptions {
  near: { type: 'Point'; coordinates: number[] };
  distanceField: string;
  maxDistance?: number;
  minDistance?: number;
  query?: Record<string, unknown>;
  spherical?: boolean;
}

export interface DatabaseStatistics {
  totalDocuments: number;
  totalSize: number;
  totalIndexes: number;
  documentCount: Map<string, number>;
  collectionSizes: Map<string, number>;
  indexSizes: Map<string, number>;
  operationsCount: Map<TransactionType, number>;
  lastOperation: number;
  uptime: number;
}

export interface DatabaseSettings {
  strictMode: boolean;
  autoCommit: boolean;
  transactionTimeout: number;
  maxTransactionSize: number;
  enableLogging: boolean;
  logLevel: LogLevel;
  enableCompression: boolean;
  enableEncryption: boolean;
  encryptionKey?: string;
  backupEnabled: boolean;
  backupInterval: number;
  maxBackups: number;
}

export type LogLevel =
  | 'debug'
  | 'info'
  | 'warning'
  | 'error'
  | 'critical';

export interface ValidationRule {
  type: 'required' | 'type' | 'min' | 'max' | 'minLength' | 'maxLength' | 'pattern' | 'enum' | 'custom';
  message?: string;
  value?: unknown;
  validator?: (value: unknown) => boolean;
}

export interface Migration {
  version: number;
  name: string;
  up: MigrationOperation[];
  down: MigrationOperation[];
}

export interface MigrationOperation {
  type: 'createCollection' | 'dropCollection' | 'renameCollection' | 'createIndex' | 'dropIndex' | 'addField' | 'removeField' | 'renameField' | 'modifyField';
  collection?: string;
  index?: string;
  field?: string;
  newName?: string;
  options?: Record<string, unknown>;
}

export interface QueryResult<T> {
  documents: T[];
  total: number;
  skipped: number;
  limit: number;
  executionTime: number;
  indexesUsed: string[];
  explain?: QueryExplanation;
}

export interface QueryExplanation {
  queryPlanner: {
    plannerVersion: string;
    namespace: string;
    indexFilterSet: boolean;
    parsedQuery: Record<string, unknown>;
    winningPlan: PlanStage;
    rejectedPlans: PlanStage[];
  };
  executionStats: {
    executionSuccess: boolean;
    nReturned: number;
    executionTimeMillis: number;
    totalKeysExamined: number;
    totalDocsExamined: number;
    executionStages: StageStats[];
  };
}

export interface PlanStage {
  stage: string;
  nReturned?: number;
  executionTimeMillisEstimate?: number;
  inputStage?: PlanStage;
  inputStages?: PlanStage[];
  direction?: string;
  indexName?: string;
  indexBounds?: Record<string, unknown>;
  filter?: string;
  docsExamined?: number;
  keysExamined?: number;
}

export interface StageStats {
  stage: string;
  nReturned: number;
  executionTimeMillisEstimate: number;
  totalDocsExamined: number;
  totalKeysExamined: number;
  childStage?: StageStats;
}

export interface BulkOperation {
  type: 'insert' | 'update' | 'delete';
  document?: Record<string, unknown>;
  filter?: Record<string, unknown>;
  update?: Record<string, unknown>;
  options?: OperationOptions;
}

export interface BulkResult {
  inserted: number;
  updated: number;
  deleted: number;
  modified: number;
  upserted: number;
  errors: BulkError[];
}

export interface BulkError {
  index: number;
  type: 'insert' | 'update' | 'delete';
  error: Error;
}

export interface PipelineBuilder<T> {
  match(query: Record<string, unknown>): PipelineBuilder<T>;
  group(grouping: Record<string, unknown>): PipelineBuilder<T>;
  sort(sorting: Record<string, 1 | -1>): PipelineBuilder<T>;
  limit(limit: number): PipelineBuilder<T>;
  skip(skip: number): PipelineBuilder<T>;
  project(fields: Record<string, unknown>): PipelineBuilder<T>;
  unwind(path: string, options?: { preserveNullAndEmptyArrays?: boolean }): PipelineBuilder<T>;
  lookup(lookup: LookupOptions): PipelineBuilder<T>;
  addFields(fields: Record<string, unknown>): PipelineBuilder<T>;
  count(name: string): PipelineBuilder<T>;
  facet(facets: Record<string, AggregationPipeline>): PipelineBuilder<T>;
  bucket(bucketOptions: BucketOptions): PipelineBuilder<T>;
  geoNear(options: GeoNearOptions): PipelineBuilder<T>;
  sample(size: number): PipelineBuilder<T>;
  unionWith(collection: string, pipeline?: AggregationPipeline): PipelineBuilder<T>;
  toPipeline(): AggregationPipeline;
}

export class GameDatabaseManager {
  private databases: Map<string, GameDatabase> = new Map();
  private activeDatabase?: GameDatabase;
  private sessionId: string = this.generateSessionId();
  private transactionQueue: TransactionLogEntry[] = [];
  private isTransactionActive: boolean = false;
  private storage: DatabaseStorageAdapter;
  private listeners: Map<string, Set<Function>> = new Map();
  private startTime: number = Date.now();

  constructor() {
    this.storage = new LocalStorageDatabaseAdapter();
  }

  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  createDatabase(name: string, schema?: DatabaseSchema): GameDatabase {
    const database: GameDatabase = {
      id: `db_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      name,
      version: schema?.version?.toString() || '1.0.0',
      schema: schema || { version: 1, collections: [], validation: [], migrations: [] },
      collections: new Map(),
      indexes: new Map(),
      transactionLog: [],
      statistics: {
        totalDocuments: 0,
        totalSize: 0,
        totalIndexes: 0,
        documentCount: new Map(),
        collectionSizes: new Map(),
        indexSizes: new Map(),
        operationsCount: new Map(),
        lastOperation: Date.now(),
        uptime: 0
      },
      settings: {
        strictMode: true,
        autoCommit: true,
        transactionTimeout: 30000,
        maxTransactionSize: 1000,
        enableLogging: true,
        logLevel: 'info',
        enableCompression: false,
        enableEncryption: false,
        backupEnabled: false,
        backupInterval: 3600000,
        maxBackups: 7
      }
    };

    if (schema?.collections) {
      schema.collections.forEach(collectionSchema => {
        this.createCollection(database, collectionSchema.name, collectionSchema);
      });
    }

    this.databases.set(name, database);

    if (!this.activeDatabase) {
      this.activeDatabase = database;
    }

    return database;
  }

  createCollection(database: GameDatabase, name: string, schema?: CollectionSchema): Collection {
    const collection: Collection = {
      name,
      documents: new Map(),
      indexes: new Map(),
      options: schema?.options || {
        capped: false,
        validationLevel: 'off',
        validationAction: 'error'
      }
    };

    if (schema?.indexes) {
      schema.indexes.forEach(indexSchema => {
        const index = this.createIndex(collection, indexSchema);
        collection.indexes.set(index.name, index);
      });
    }

    database.collections.set(name, collection);
    database.statistics.totalDocuments = this.calculateTotalDocuments(database);
    database.statistics.collectionSizes.set(name, 0);

    return collection;
  }

  private createIndex(collection: Collection, schema: IndexSchema): Index {
    const index: Index = {
      id: `idx_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      name: schema.fields.join('_'),
      fields: schema.fields,
      type: schema.type,
      unique: schema.unique,
      sparse: schema.sparse,
      entries: new Map(),
      size: 0
    };

    return index;
  }

  insert<T extends Record<string, unknown>>(
    collectionName: string,
    document: T,
    options?: OperationOptions
  ): string {
    if (!this.activeDatabase) {
      throw new Error('No active database');
    }

    const collection = this.activeDatabase.collections.get(collectionName);
    if (!collection) {
      throw new Error(`Collection ${collectionName} not found`);
    }

    this.validateDocument(collection, document);

    const docId = (document._id as string) || this.generateDocumentId();
    const now = Date.now();

    const newDocument: Document = {
      _id: docId,
      _createdAt: now,
      _updatedAt: now,
      _version: 1,
      _deleted: false,
      data: document
    };

    if (this.activeDatabase.settings.enableEncryption && this.activeDatabase.settings.encryptionKey) {
      newDocument._checksum = this.computeChecksum(document);
    }

    collection.documents.set(docId, newDocument);

    this.updateIndexes(collection, newDocument);

    this.activeDatabase.statistics.totalDocuments++;
    this.activeDatabase.statistics.lastOperation = Date.now();

    const operation: TransactionLogEntry = {
      id: `log_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      type: 'insert',
      collection: collectionName,
      documentId: docId,
      timestamp: Date.now(),
      operation: { type: 'insert' },
      after: document,
      sessionId: this.sessionId
    };

    if (this.activeDatabase.settings.enableLogging) {
      this.logOperation(operation);
    }

    this.activeDatabase.transactionLog.push(operation);
    this.trimTransactionLog();

    this.emit('document_inserted', { collection: collectionName, documentId: docId });

    return docId;
  }

  find<T extends Record<string, unknown>>(
    collectionName: string,
    query: Record<string, unknown>,
    options?: FindOptions
  ): QueryResult<T> {
    if (!this.activeDatabase) {
      throw new Error('No active database');
    }

    const collection = this.activeDatabase.collections.get(collectionName);
    if (!collection) {
      throw new Error(`Collection ${collectionName} not found`);
    }

    const startTime = performance.now();
    const limit = options?.limit || 0;
    const skip = options?.skip || 0;

    let documents = Array.from(collection.documents.values())
      .filter(doc => !doc._deleted)
      .filter(doc => this.matchDocument(doc.data, query));

    if (options?.sort) {
      documents = this.sortDocuments(documents, options.sort);
    }

    const total = documents.length;

    if (skip > 0) {
      documents = documents.slice(skip);
    }

    if (limit > 0) {
      documents = documents.slice(0, limit);
    }

    const executionTime = performance.now() - startTime;

    this.activeDatabase.statistics.lastOperation = Date.now();
    const opCount = this.activeDatabase.statistics.operationsCount;
    opCount.set('insert', (opCount.get('insert') || 0) + 1);

    return {
      documents: documents.map(doc => doc.data as T),
      total,
      skipped: skip,
      limit,
      executionTime,
      indexesUsed: this.getUsedIndexes(collection, query)
    };
  }

  findOne<T extends Record<string, unknown>>(
    collectionName: string,
    query: Record<string, unknown>
  ): T | null {
    const result = this.find<T>(collectionName, query, { limit: 1 });
    return result.documents[0] || null;
  }

  update(
    collectionName: string,
    query: Record<string, unknown>,
    update: Record<string, unknown>,
    options?: OperationOptions
  ): number {
    if (!this.activeDatabase) {
      throw new Error('No active database');
    }

    const collection = this.activeDatabase.collections.get(collectionName);
    if (!collection) {
      throw new Error(`Collection ${collectionName} not found`);
    }

    const documents = Array.from(collection.documents.values())
      .filter(doc => !doc._deleted)
      .filter(doc => this.matchDocument(doc.data, query));

    let updatedCount = 0;

    documents.forEach(doc => {
      if (options?.upsert && documents.length === 0) {
        const newDoc = { ...query, ...update };
        this.insert(collectionName, newDoc);
        updatedCount++;
      } else {
        const before = { ...doc.data };
        doc.data = this.applyUpdate(doc.data, update);
        doc._updatedAt = Date.now();
        doc._version++;

        if (this.activeDatabase!.settings.enableEncryption) {
          doc._checksum = this.computeChecksum(doc.data);
        }

        this.updateIndexesForDocument(collection, doc);

        const operation: TransactionLogEntry = {
          id: `log_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
          type: 'update',
          collection: collectionName,
          documentId: doc._id,
          timestamp: Date.now(),
          operation: { type: 'update', query, update },
          before,
          after: doc.data,
          sessionId: this.sessionId
        };

        if (this.activeDatabase!.settings.enableLogging) {
          this.logOperation(operation);
        }

        this.activeDatabase!.transactionLog.push(operation);
        updatedCount++;

        this.emit('document_updated', { collection: collectionName, documentId: doc._id });
      }
    });

    this.trimTransactionLog();

    this.activeDatabase.statistics.lastOperation = Date.now();
    const opCount = this.activeDatabase.statistics.operationsCount;
    opCount.set('update', (opCount.get('update') || 0) + updatedCount);

    return updatedCount;
  }

  delete(
    collectionName: string,
    query: Record<string, unknown>,
    options?: OperationOptions
  ): number {
    if (!this.activeDatabase) {
      throw new Error('No active database');
    }

    const collection = this.activeDatabase.collections.get(collectionName);
    if (!collection) {
      throw new Error(`Collection ${collectionName} not found`);
    }

    const documents = Array.from(collection.documents.values())
      .filter(doc => !doc._deleted)
      .filter(doc => this.matchDocument(doc.data, query));

    let deletedCount = 0;

    documents.forEach(doc => {
      if (options?.multi) {
        doc._deleted = true;
        doc._updatedAt = Date.now();

        const operation: TransactionLogEntry = {
          id: `log_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
          type: 'delete',
          collection: collectionName,
          documentId: doc._id,
          timestamp: Date.now(),
          operation: { type: 'delete', query },
          before: doc.data,
          sessionId: this.sessionId
        };

        if (this.activeDatabase!.settings.enableLogging) {
          this.logOperation(operation);
        }

        this.activeDatabase!.transactionLog.push(operation);
      } else {
        collection.documents.delete(doc._id);
        deletedCount++;

        const operation: TransactionLogEntry = {
          id: `log_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
          type: 'delete',
          collection: collectionName,
          documentId: doc._id,
          timestamp: Date.now(),
          operation: { type: 'delete', query },
          before: doc.data,
          sessionId: this.sessionId
        };

        if (this.activeDatabase!.settings.enableLogging) {
          this.logOperation(operation);
        }

        this.activeDatabase!.transactionLog.push(operation);

        this.emit('document_deleted', { collection: collectionName, documentId: doc._id });
      }

      deletedCount++;
    });

    this.trimTransactionLog();

    if (!options?.multi) {
      this.activeDatabase.statistics.totalDocuments -= deletedCount;
    }

    this.activeDatabase.statistics.lastOperation = Date.now();
    const opCount = this.activeDatabase.statistics.operationsCount;
    opCount.set('delete', (opCount.get('delete') || 0) + deletedCount);

    return deletedCount;
  }

  count(collectionName: string, query?: Record<string, unknown>): number {
    if (query) {
      const result = this.find(collectionName, query);
      return result.total;
    }

    const collection = this.activeDatabase?.collections.get(collectionName);
    if (!collection) {
      return 0;
    }

    return Array.from(collection.documents.values()).filter(doc => !doc._deleted).length;
  }

  distinct<T>(collectionName: string, field: string, query?: Record<string, unknown>): T[] {
    const documents = query
      ? this.find(collectionName, query).documents
      : this.find(collectionName, {}).documents;

    const values = new Set<T>();
    documents.forEach(doc => {
      const value = this.getNestedValue(doc, field);
      if (value !== undefined) {
        values.add(value as T);
      }
    });

    return Array.from(values);
  }

  aggregate<T extends Record<string, unknown>>(
    collectionName: string,
    pipeline: AggregationPipeline
  ): T[] {
    if (!this.activeDatabase) {
      throw new Error('No active database');
    }

    const collection = this.activeDatabase.collections.get(collectionName);
    if (!collection) {
      throw new Error(`Collection ${collectionName} not found`);
    }

    let results = Array.from(collection.documents.values())
      .filter(doc => !doc._deleted)
      .map(doc => doc.data as Record<string, unknown>);

    pipeline.stages.forEach(stage => {
      if (stage.$match) {
        results = results.filter(doc => this.matchDocument(doc, stage.$match));
      } else if (stage.$project) {
        results = results.map(doc => this.projectDocument(doc, stage.$project!));
      } else if (stage.$group) {
        results = this.groupDocuments(results, stage.$group);
      } else if (stage.$sort) {
        results = this.sortResults(results, stage.$sort);
      } else if (stage.$limit) {
        results = results.slice(0, stage.$limit);
      } else if (stage.$skip) {
        results = results.slice(stage.$skip);
      } else if (stage.$unwind) {
        results = this.unwindDocuments(results, stage.$unwind);
      } else if (stage.$addFields) {
        results = results.map(doc => ({ ...doc, ...stage.$addFields }));
      } else if (stage.$count) {
        const count = results.length;
        results = [{ [stage.$count]: count }] as Record<string, unknown>[];
      }
    });

    return results as T[];
  }

  private matchDocument(document: Record<string, unknown>, query: Record<string, unknown>): boolean {
    for (const [key, value] of Object.entries(query)) {
      if (key.startsWith('$')) {
        continue;
      }

      const docValue = this.getNestedValue(document, key);

      if (typeof value === 'object' && value !== null) {
        if ('$eq' in value && docValue !== value.$eq) return false;
        if ('$ne' in value && docValue === value.$ne) return false;
        if ('$gt' in value && !(docValue as number) > (value.$gt as number)) return false;
        if ('$gte' in value && !(docValue as number) >= (value.$gte as number)) return false;
        if ('$lt' in value && !(docValue as number) < (value.$lt as number)) return false;
        if ('$lte' in value && !(docValue as number) <= (value.$lte as number)) return false;
        if ('$in' in value && !(value.$in as unknown[]).includes(docValue)) return false;
        if ('$nin' in value && (value.$nin as unknown[]).includes(docValue)) return false;
        if ('$exists' in value && (value.$exists ? docValue === undefined : docValue !== undefined)) return false;
        if ('$regex' in value && typeof docValue === 'string') {
          const regex = new RegExp(value.$regex as string, (value.$options as string) || '');
          if (!regex.test(docValue)) return false;
        }
      } else {
        if (docValue !== value) return false;
      }
    }

    return true;
  }

  private getNestedValue(document: Record<string, unknown>, path: string): unknown {
    return path.split('.').reduce((obj: unknown, key: string) => {
      if (obj && typeof obj === 'object') {
        return (obj as Record<string, unknown>)[key];
      }
      return undefined;
    }, document);
  }

  private sortDocuments(documents: Document[], sorting: Record<string, 1 | -1>): Document[] {
    return [...documents].sort((a, b) => {
      for (const [field, order] of Object.entries(sorting)) {
        const aValue = this.getNestedValue(a.data, field);
        const bValue = this.getNestedValue(b.data, field);

        if (aValue === bValue) continue;
        if (aValue === undefined) return order === 1 ? 1 : -1;
        if (bValue === undefined) return order === 1 ? -1 : 1;

        const comparison = aValue < bValue ? -1 : 1;
        return order === 1 ? comparison : -comparison;
      }
      return 0;
    });
  }

  private sortResults(documents: Record<string, unknown>[], sorting: Record<string, 1 | -1>): Record<string, unknown>[] {
    return [...documents].sort((a, b) => {
      for (const [field, order] of Object.entries(sorting)) {
        const aValue = this.getNestedValue(a, field);
        const bValue = this.getNestedValue(b, field);

        if (aValue === bValue) continue;
        if (aValue === undefined) return order === 1 ? 1 : -1;
        if (bValue === undefined) return order === 1 ? -1 : 1;

        const comparison = aValue < bValue ? -1 : 1;
        return order === 1 ? comparison : -comparison;
      }
      return 0;
    });
  }

  private applyUpdate(document: Record<string, unknown>, update: Record<string, unknown>): Record<string, unknown> {
    const updated = { ...document };

    for (const [key, value] of Object.entries(update)) {
      if (key.startsWith('$set')) {
        const setOps = value as Record<string, unknown>;
        for (const [field, val] of Object.entries(setOps)) {
          this.setNestedValue(updated, field, val);
        }
      } else if (key.startsWith('$unset')) {
        const unsetOps = value as Record<string, unknown>;
        for (const field of Object.keys(unsetOps)) {
          this.unsetNestedValue(updated, field);
        }
      } else if (key.startsWith('$inc')) {
        const incOps = value as Record<string, number>;
        for (const [field, val] of Object.entries(incOps)) {
          const current = this.getNestedValue(updated, field) as number || 0;
          this.setNestedValue(updated, field, current + val);
        }
      } else if (key.startsWith('$mul')) {
        const mulOps = value as Record<string, number>;
        for (const [field, val] of Object.entries(mulOps)) {
          const current = this.getNestedValue(updated, field) as number || 0;
          this.setNestedValue(updated, field, current * val);
        }
      } else if (key.startsWith('$push')) {
        const pushOps = value as Record<string, unknown>;
        for (const [field, val] of Object.entries(pushOps)) {
          const current = this.getNestedValue(updated, field) as unknown[] || [];
          current.push(val);
          this.setNestedValue(updated, field, current);
        }
      } else if (key.startsWith('$pull')) {
        const pullOps = value as Record<string, unknown>;
        for (const [field, condition] of Object.entries(pullOps)) {
          const current = this.getNestedValue(updated, field) as unknown[] || [];
          const filtered = current.filter(item => !this.matchDocument(item as Record<string, unknown>, condition as Record<string, unknown>));
          this.setNestedValue(updated, field, filtered);
        }
      } else if (!key.startsWith('$')) {
        updated[key] = value;
      }
    }

    return updated;
  }

  private setNestedValue(document: Record<string, unknown>, path: string, value: unknown): void {
    const parts = path.split('.');
    let current = document;

    for (let i = 0; i < parts.length - 1; i++) {
      const part = parts[i];
      if (!(part in current) || typeof current[part] !== 'object') {
        current[part] = {};
      }
      current = current[part] as Record<string, unknown>;
    }

    current[parts[parts.length - 1]] = value;
  }

  private unsetNestedValue(document: Record<string, unknown>, path: string): void {
    const parts = path.split('.');
    let current = document;

    for (let i = 0; i < parts.length - 1; i++) {
      const part = parts[i];
      if (!(part in current) || typeof current[part] !== 'object') {
        return;
      }
      current = current[part] as Record<string, unknown>;
    }

    delete current[parts[parts.length - 1]];
  }

  private projectDocument(document: Record<string, unknown>, projection: Record<string, unknown>): Record<string, unknown> {
    const result: Record<string, unknown> = {};

    for (const [field, include] of Object.entries(projection)) {
      if (field === '_id' && include === 0) {
        continue;
      }

      if (include === 1 || include === true) {
        result[field] = this.getNestedValue(document, field);
      } else if (include === 0 || include === false) {
      }
    }

    return result;
  }

  private groupDocuments(documents: Record<string, unknown>[], grouping: Record<string, unknown>): Record<string, unknown>[] {
    const groups = new Map<string, Record<string, unknown[]>>();

    for (const doc of documents) {
      let groupKey = '';

      for (const [key, value] of Object.entries(grouping)) {
        if (key === '_id') {
          if (typeof value === 'string') {
            groupKey += this.getNestedValue(doc, value) + '_';
          } else if (typeof value === 'object') {
            const parts: string[] = [];
            for (const [field, expr] of Object.entries(value as Record<string, unknown>)) {
              if (typeof expr === 'object' && expr !== null) {
                for (const [op, fieldName] of Object.entries(expr)) {
                  if (op === '$first' || op === '$last' || op === '$sum' || op === '$avg') {
                    parts.push(String(this.getNestedValue(doc, fieldName as string)));
                  }
                }
              }
            }
            groupKey += parts.join('_');
          }
        } else {
          const expr = value as Record<string, unknown>;
          let result: unknown;

          for (const [op, field] of Object.entries(expr)) {
            const fieldName = field as string;
            const fieldValue = this.getNestedValue(doc, fieldName);

            switch (op) {
              case '$sum':
                result = (result as number || 0) + (fieldValue as number || 0);
                break;
              case '$avg':
                result = fieldValue;
                break;
              case '$min':
                result = result === undefined ? fieldValue : Math.min(result as number, fieldValue as number);
                break;
              case '$max':
                result = result === undefined ? fieldValue : Math.max(result as number, fieldValue as number);
                break;
              case '$first':
                result = result === undefined ? fieldValue : result;
                break;
              case '$last':
                result = fieldValue;
                break;
            }
          }

          result = result ?? 0;
          groupKey += `${key}:${result}_`;
        }
      }

      if (!groups.has(groupKey)) {
        groups.set(groupKey, { _id: groupKey });
      }

      const group = groups.get(groupKey)!;

      for (const [key, value] of Object.entries(grouping)) {
        if (key === '_id') continue;

        const expr = value as Record<string, unknown>;

        for (const [op, field] of Object.entries(expr)) {
          const fieldName = field as string;
          const fieldValue = this.getNestedValue(doc, fieldName);

          switch (op) {
            case '$sum':
              if (typeof fieldValue === 'number') {
                group[key] = ((group[key] as number) || 0) + fieldValue;
              }
              break;
            case '$avg':
              if (!group[`${key}_count`]) {
                group[`${key}_count`] = 0;
                group[key] = 0;
              }
              (group[key] as number) += fieldValue as number;
              (group[`${key}_count`] as number)++;
              break;
            case '$min':
              if (group[key] === undefined) {
                group[key] = fieldValue;
              } else {
                group[key] = Math.min(group[key] as number, fieldValue as number);
              }
              break;
            case '$max':
              if (group[key] === undefined) {
                group[key] = fieldValue;
              } else {
                group[key] = Math.max(group[key] as number, fieldValue as number);
              }
              break;
          }
        }
      }
    }

    const results = Array.from(groups.values());

    results.forEach(group => {
      for (const key of Object.keys(group)) {
        if (key.endsWith('_count')) {
          const countField = key.replace('_count', '');
          if (group[countField] !== undefined && typeof group[countField] === 'number') {
            group[countField] = (group[countField] as number) / (group[key] as number);
          }
          delete group[key];
        }
      }
    });

    return results;
  }

  private unwindDocuments(documents: Record<string, unknown>[], unwind: string | { path: string; preserveNullAndEmptyArrays?: boolean }): Record<string, unknown>[] {
    const path = typeof unwind === 'string' ? unwind : unwind.path;
    const preserveNull = typeof unwind === 'object' && unwind.preserveNullAndEmptyArrays;

    const results: Record<string, unknown>[] = [];

    for (const doc of documents) {
      const arrayValue = this.getNestedValue(doc, path);

      if (arrayValue === undefined || arrayValue === null) {
        if (preserveNull) {
          results.push(doc);
        }
        continue;
      }

      if (!Array.isArray(arrayValue)) {
        continue;
      }

      if (arrayValue.length === 0 && !preserveNull) {
        continue;
      }

      if (arrayValue.length === 0 && preserveNull) {
        results.push(doc);
        continue;
      }

      for (const item of arrayValue) {
        const newDoc = { ...doc };
        const parts = path.split('.');
        let current = newDoc;

        for (let i = 0; i < parts.length - 1; i++) {
          current = current[parts[i]] as Record<string, unknown>;
        }

        current[parts[parts.length - 1]] = item;
        results.push(newDoc);
      }
    }

    return results;
  }

  private getUsedIndexes(collection: Collection, query: Record<string, unknown>): string[] {
    const usedIndexes: string[] = [];

    collection.indexes.forEach((index, name) => {
      for (const field of index.fields) {
        if (field in query) {
          usedIndexes.push(name);
          break;
        }
      }
    });

    return usedIndexes;
  }

  private updateIndexes(collection: Collection, document: Document): void {
    collection.indexes.forEach(index => {
      const key = this.getIndexKey(document.data, index.fields);
      const existing = index.entries.get(key) || [];
      existing.push(document._id);
      index.entries.set(key, existing);
    });
  }

  private updateIndexesForDocument(collection: Collection, document: Document): void {
    collection.indexes.forEach(index => {
      const key = this.getIndexKey(document.data, index.fields);

      for (const [existingKey, docIds] of index.entries.entries()) {
        if (docIds.includes(document._id)) {
          if (existingKey !== key) {
            const filtered = docIds.filter(id => id !== document._id);
            if (filtered.length > 0) {
              index.entries.set(existingKey, filtered);
            } else {
              index.entries.delete(existingKey);
            }
          }
          break;
        }
      }

      const newEntries = index.entries.get(key) || [];
      if (!newEntries.includes(document._id)) {
        newEntries.push(document._id);
        index.entries.set(key, newEntries);
      }
    });
  }

  private getIndexKey(document: Record<string, unknown>, fields: string[]): string {
    return fields.map(field => {
      const value = this.getNestedValue(document, field);
      return String(value ?? 'null');
    }).join('_');
  }

  private validateDocument(collection: Collection, document: Record<string, unknown>): void {
    if (!collection.options.validator) return;

    if (collection.options.validationLevel === 'off') return;

    const errors: string[] = [];

    for (const [field, rule] of Object.entries(collection.options.validator)) {
      const value = this.getNestedValue(document, field);

      if (rule.type === 'required' && (value === undefined || value === null)) {
        errors.push(`${field} is required`);
      }

      if (rule.type === 'type' && value !== undefined && typeof value !== (rule.value as string)) {
        errors.push(`${field} must be of type ${rule.value}`);
      }

      if (rule.type === 'minLength' && Array.isArray(value) && value.length < (rule.value as number)) {
        errors.push(`${field} must have at least ${rule.value} elements`);
      }

      if (rule.type === 'maxLength' && Array.isArray(value) && value.length > (rule.value as number)) {
        errors.push(`${field} must have at most ${rule.value} elements`);
      }
    }

    if (errors.length > 0) {
      if (collection.options.validationAction === 'error') {
        throw new Error(`Validation failed: ${errors.join(', ')}`);
      }
    }
  }

  private calculateTotalDocuments(database: GameDatabase): number {
    let total = 0;
    database.collections.forEach(collection => {
      total += Array.from(collection.documents.values()).filter(doc => !doc._deleted).length;
    });
    return total;
  }

  private generateDocumentId(): string {
    return `doc_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private computeChecksum(document: Record<string, unknown>): string {
    const str = JSON.stringify(document);
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash;
    }
    return Math.abs(hash).toString(16);
  }

  private logOperation(operation: TransactionLogEntry): void {
    if (this.activeDatabase?.settings.enableLogging) {
      console.log(`[${operation.type.toUpperCase()}] ${operation.collection}.${operation.operation.type}`, {
        timestamp: new Date(operation.timestamp).toISOString(),
        documentId: operation.documentId
      });
    }
  }

  private trimTransactionLog(): void {
    if (!this.activeDatabase) return;

    const maxLogSize = 10000;
    if (this.activeDatabase.transactionLog.length > maxLogSize) {
      this.activeDatabase.transactionLog = this.activeDatabase.transactionLog.slice(-maxLogSize);
    }
  }

  beginTransaction(): void {
    this.isTransactionActive = true;
    this.transactionQueue = [];
  }

  commitTransaction(): void {
    if (!this.isTransactionActive) return;

    this.transactionQueue.forEach(entry => {
      if (this.activeDatabase) {
        this.activeDatabase.transactionLog.push(entry);
      }
    });

    this.isTransactionActive = false;
    this.transactionQueue = [];
  }

  rollbackTransaction(): void {
    if (!this.isTransactionActive) return;

    for (let i = this.transactionQueue.length - 1; i >= 0; i--) {
      const entry = this.transactionQueue[i];
      this.applyRollback(entry);
    }

    this.isTransactionActive = false;
    this.transactionQueue = [];
  }

  private applyRollback(entry: TransactionLogEntry): void {
    if (!this.activeDatabase) return;

    const collection = this.activeDatabase.collections.get(entry.collection);
    if (!collection) return;

    switch (entry.type) {
      case 'insert':
        if (entry.documentId) {
          collection.documents.delete(entry.documentId);
        }
        break;

      case 'update':
        if (entry.documentId && entry.before) {
          const doc = collection.documents.get(entry.documentId);
          if (doc) {
            doc.data = entry.before as Record<string, unknown>;
            doc._updatedAt = Date.now();
            doc._version--;
          }
        }
        break;

      case 'delete':
        if (entry.documentId && entry.before) {
          const doc: Document = {
            _id: entry.documentId,
            _createdAt: Date.now(),
            _updatedAt: Date.now(),
            _version: 1,
            _deleted: false,
            data: entry.before as Record<string, unknown>
          };
          collection.documents.set(entry.documentId, doc);
        }
        break;
    }

    entry.rollback = true;
  }

  createPipeline<T extends Record<string, unknown>>(): PipelineBuilder<T> {
    const stages: AggregationStage[] = [];

    return {
      match(query: Record<string, unknown>): PipelineBuilder<T> {
        stages.push({ $match: query });
        return this;
      },
      group(grouping: Record<string, unknown>): PipelineBuilder<T> {
        stages.push({ $group: grouping });
        return this;
      },
      sort(sorting: Record<string, 1 | -1>): PipelineBuilder<T> {
        stages.push({ $sort: sorting });
        return this;
      },
      limit(limit: number): PipelineBuilder<T> {
        stages.push({ $limit: limit });
        return this;
      },
      skip(skip: number): PipelineBuilder<T> {
        stages.push({ $skip: skip });
        return this;
      },
      project(fields: Record<string, unknown>): PipelineBuilder<T> {
        stages.push({ $project: fields });
        return this;
      },
      unwind(path: string, options?: { preserveNullAndEmptyArrays?: boolean }): PipelineBuilder<T> {
        stages.push({ $unwind: options ? { path, ...options } : path });
        return this;
      },
      lookup(lookup: LookupOptions): PipelineBuilder<T> {
        stages.push({ $lookup: lookup });
        return this;
      },
      addFields(fields: Record<string, unknown>): PipelineBuilder<T> {
        stages.push({ $addFields: fields });
        return this;
      },
      count(name: string): PipelineBuilder<T> {
        stages.push({ $count: name });
        return this;
      },
      facet(facets: Record<string, AggregationPipeline>): PipelineBuilder<T> {
        stages.push({ $facet: facets });
        return this;
      },
      bucket(bucketOptions: BucketOptions): PipelineBuilder<T> {
        stages.push({ $bucket: bucketOptions });
        return this;
      },
      geoNear(options: GeoNearOptions): PipelineBuilder<T> {
        stages.push({ $geoNear: options });
        return this;
      },
      sample(size: number): PipelineBuilder<T> {
        stages.push({ $sample: { size } });
        return this;
      },
      unionWith(collection: string, pipeline?: AggregationPipeline): PipelineBuilder<T> {
        stages.push({ $unionWith: { coll: collection, pipeline } });
        return this;
      },
      toPipeline(): AggregationPipeline {
        return { stages: [...stages] };
      }
    };
  }

  bulkWrite(
    collectionName: string,
    operations: BulkOperation[]
  ): BulkResult {
    const result: BulkResult = {
      inserted: 0,
      updated: 0,
      deleted: 0,
      modified: 0,
      upserted: 0,
      errors: []
    };

    operations.forEach((op, index) => {
      try {
        switch (op.type) {
          case 'insert':
            if (op.document) {
              this.insert(collectionName, op.document, op.options);
              result.inserted++;
            }
            break;

          case 'update':
            if (op.filter && op.update) {
              const updated = this.update(collectionName, op.filter, op.update, op.options);
              result.updated += updated;
            }
            break;

          case 'delete':
            if (op.filter) {
              const deleted = this.delete(collectionName, op.filter, op.options);
              result.deleted += deleted;
            }
            break;
        }
      } catch (error) {
        result.errors.push({
          index,
          type: op.type,
          error: error as Error
        });
      }
    });

    return result;
  }

  dropCollection(collectionName: string): boolean {
    if (!this.activeDatabase) return false;

    const deleted = this.activeDatabase.collections.delete(collectionName);

    if (deleted) {
      this.activeDatabase.statistics.totalDocuments = this.calculateTotalDocuments(this.activeDatabase);
      this.emit('collection_dropped', { collection: collectionName });
    }

    return deleted;
  }

  dropDatabase(databaseName: string): boolean {
    const deleted = this.databases.delete(databaseName);

    if (deleted && this.activeDatabase?.name === databaseName) {
      this.activeDatabase = undefined;
    }

    return deleted;
  }

  getStatistics(): DatabaseStatistics | null {
    return this.activeDatabase?.statistics || null;
  }

  getCollectionNames(): string[] {
    if (!this.activeDatabase) return [];
    return Array.from(this.activeDatabase.collections.keys());
  }

  exportCollection(collectionName: string): string {
    if (!this.activeDatabase) return '[]';

    const collection = this.activeDatabase.collections.get(collectionName);
    if (!collection) return '[]';

    const documents = Array.from(collection.documents.values())
      .filter(doc => !doc._deleted)
      .map(doc => doc.data);

    return JSON.stringify(documents, null, 2);
  }

  importCollection(collectionName: string, jsonData: string): number {
    try {
      const documents = JSON.parse(jsonData);

      if (!Array.isArray(documents)) {
        throw new Error('Import data must be an array');
      }

      if (!this.activeDatabase) {
        throw new Error('No active database');
      }

      let imported = 0;

      documents.forEach((doc: Record<string, unknown>) => {
        this.insert(collectionName, doc);
        imported++;
      });

      return imported;
    } catch (error) {
      throw new Error(`Import failed: ${(error as Error).message}`);
    }
  }

  backup(): string {
    if (!this.activeDatabase) return '{}';

    const backup = {
      ...this.activeDatabase,
      collections: new Map(),
      statistics: {
        ...this.activeDatabase.statistics,
        documentCount: new Map(),
        collectionSizes: new Map(),
        indexSizes: new Map(),
        operationsCount: new Map()
      }
    };

    this.activeDatabase.collections.forEach((collection, name) => {
      const collectionData = {
        name: collection.name,
        documents: Array.from(collection.documents.values()),
        options: collection.options
      };
      backup.collections.set(name, collectionData as unknown as Collection);
    });

    return JSON.stringify(backup);
  }

  restore(backupData: string): boolean {
    try {
      const backup = JSON.parse(backupData);

      if (!backup.name || !backup.collections) {
        throw new Error('Invalid backup format');
      }

      this.createDatabase(backup.name, backup.schema);

      const database = this.databases.get(backup.name);
      if (!database) return false;

      for (const [name, collectionData] of Object.entries(backup.collections as Record<string, unknown>)) {
        const data = collectionData as Record<string, unknown>;
        const docs = data.documents as Record<string, unknown>[];

        docs.forEach(doc => {
          this.insert(name, doc as Record<string, unknown>);
        });
      }

      return true;
    } catch (error) {
      console.error('Restore failed:', error);
      return false;
    }
  }

  setActiveDatabase(name: string): boolean {
    const database = this.databases.get(name);
    if (!database) return false;

    this.activeDatabase = database;
    return true;
  }

  getActiveDatabase(): GameDatabase | undefined {
    return this.activeDatabase;
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  clear(): void {
    this.databases.clear();
    this.activeDatabase = undefined;
  }
}

interface FindOptions {
  limit?: number;
  skip?: number;
  sort?: Record<string, 1 | -1>;
  projection?: Record<string, unknown>;
}

class DatabaseStorageAdapter {
  get(key: string): string | null {
    return localStorage.getItem(key);
  }

  set(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  remove(key: string): void {
    localStorage.removeItem(key);
  }

  keys(): string[] {
    const keys: string[] = [];
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key) keys.push(key);
    }
    return keys;
  }

  clear(): void {
    localStorage.clear();
  }
}

class LocalStorageDatabaseAdapter extends DatabaseStorageAdapter {}

export const gameDatabase = new GameDatabaseManager();
