<script lang="ts">
    import {
        WalterGrid,
        WalterHeader,
        WalterLinkTile
    } from '$walter/components';
    import { ClickableTile } from 'carbon-components-svelte';
</script>

<WalterHeader title="Adminbereich" />
<WalterGrid>
    <ClickableTile href="/accounts">Nutzeraccounts</ClickableTile>
</WalterGrid>
