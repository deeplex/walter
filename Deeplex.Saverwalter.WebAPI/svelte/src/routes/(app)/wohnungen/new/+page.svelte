<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterWohnung
    } from '$WalterComponents';
    import type { WalterWohnungEntry } from '$WalterLib';
    import type { PageData } from './$types';

    export let data: PageData;
    const entry: Partial<WalterWohnungEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterWohnung kontakte={data.kontakte} a={entry} />
</WalterGrid>
