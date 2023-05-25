<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterZaehlerstandEntry } from '$walter/lib';
    import WalterTextInput from '../elements/WalterTextInput.svelte';

    export let a: Partial<WalterZaehlerstandEntry> = {};
</script>

<Row>
    <WalterNumberInput bind:value={a.stand} label="Zählerstand" />
    <WalterDatePicker bind:value={a.datum} labelText="Ablesedatum" />
    <WalterTextInput readonly bind:value={a.einheit} labelText="Einheit" />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={a.notiz} />
</Row>
