<script lang="ts">
    import type { PageData } from './$types';
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterErhaltungsaufwendung
    } from '$walter/components';

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.aussteller?.text + ' - ' + data.entry.bezeichnung}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterErhaltungsaufwendung
        kontakte={data.kontakte}
        wohnungen={data.wohnungen}
        entry={data.entry}
    />
</WalterGrid>
