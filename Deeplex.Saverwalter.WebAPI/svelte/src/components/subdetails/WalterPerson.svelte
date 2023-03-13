<script lang="ts">
	import { Row } from 'carbon-components-svelte';

	import type { WalterPersonEntry, WalterSelectionEntry } from '$WalterTypes';
	import { WalterAdresse, WalterTextInput } from '$WalterComponents';

	export let value: Partial<WalterPersonEntry>;
</script>

<WalterAdresse bind:value={value.adresse} />
<Row>
	<WalterTextInput bind:value={value.email} labelText="E-Mail" />
	<WalterTextInput bind:value={value.fax} labelText="Fax" />
</Row>
<Row>
	<WalterTextInput bind:value={value.telefon} labelText="Telefon" />
	<WalterTextInput bind:value={value.mobil} labelText="Mobil" />
</Row>
