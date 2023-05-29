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
    a={data.a}
    apiURL={data.apiURL}
    title={data.a.aussteller?.text + ' - ' + data.a.bezeichnung}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterErhaltungsaufwendung
        kontakte={data.kontakte}
        wohnungen={data.wohnungen}
        a={data.a}
    />
</WalterGrid>
