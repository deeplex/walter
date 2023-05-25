export class WalterToastContent {
    constructor(
        public successTitle: string,
        public failureTitle: string = '',
        public subtitleSuccess: (...args: any) => string = () => '',
        public subtitleFailure: (...args: any) => string = () => ''
    ) {}
}
