export class WalterToastContent {
    constructor(
        public successTitle: string,
        public failureTitle: string = '',
        public subtitleSuccess: (...args: unknown[]) => string = () => '',
        public subtitleFailure: (...args: unknown[]) => string = () => ''
    ) {}
}
