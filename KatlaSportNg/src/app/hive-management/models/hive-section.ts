export class HiveSection {
    constructor(
        public id: number,
        public code: string,
        public name: string,
        public isDelited: boolean,
        public lastUpdate: string,
        public hiveId: number
    ) { }
}
