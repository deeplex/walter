export class WalterToastContent {
    constructor(
        public successTitle: string | undefined,
        public failureTitle: string | undefined,
        public subtitleSuccess: (...args: unknown[]) => string = () => '',
        public subtitleFailure: (...args: unknown[]) => string = () => ''
    ) {}
}
