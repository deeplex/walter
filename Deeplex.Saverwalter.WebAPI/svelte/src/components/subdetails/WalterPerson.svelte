<script lang="ts">
    import { Row } from 'carbon-components-svelte';

    import type { WalterPersonEntry } from '$walter/lib';
    import { WalterAdresse, WalterTextInput } from '$walter/components';

    export let value: Partial<WalterPersonEntry>;
    export let readonly = false;
</script>

<WalterAdresse {readonly} bind:entry={value.adresse} />
<Row>
    <WalterTextInput {readonly} bind:value={value.email} labelText="E-Mail" />
    <WalterTextInput {readonly} bind:value={value.fax} labelText="Fax" />
</Row>
<Row>
    <WalterTextInput {readonly} bind:value={value.telefon} labelText="Telefon" />
    <WalterTextInput {readonly} bind:value={value.mobil} labelText="Mobil" />
</Row>
