<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterZaehlerstand
    } from '$walter/components';
    import type { WalterZaehlerstandEntry } from '$walter/lib';

    const apiURL = `/api/zaehlerstaende`;
    const title = 'Neuer Zählerstand';

    const entry: Partial<WalterZaehlerstandEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
    <WalterZaehlerstand {entry} />
</WalterGrid>
