<script lang="ts">
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { ClickableTile } from 'carbon-components-svelte';

    export let href: string;
    export let name: string;
    export let fileWrapper: WalterS3FileWrapper;

    fileWrapper.register(name, href);
</script>

<ClickableTile {href}>{name}</ClickableTile>
