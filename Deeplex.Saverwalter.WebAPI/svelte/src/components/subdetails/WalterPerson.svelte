<script lang="ts">
	import { Row } from 'carbon-components-svelte';

	import type { WalterPersonEntry, WalterSelectionEntry } from '$WalterTypes';
	import {
		WalterAdresse,
		WalterMultiSelect,
		WalterTextArea,
		WalterTextInput
	} from '$WalterComponents';

	export let value: Partial<WalterPersonEntry>;
	export let juristischePersonen: WalterSelectionEntry[];
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
<Row>
	<WalterMultiSelect
		bind:a={juristischePersonen}
		titleText="Juristische Personen"
		bind:value={value.selectedJuristischePersonen}
	/>
</Row>
<slot />
<Row>
	<WalterTextArea bind:value={value.notiz} labelText="Notiz" />
</Row>
