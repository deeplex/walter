<script lang="ts">
	import { Row } from 'carbon-components-svelte';

	import type { WalterPersonEntry } from '$WalterTypes';
	import { WalterAdresse, WalterTextInput } from '$WalterComponents';

	export let person: Promise<WalterPersonEntry> | undefined = undefined;
	export let binding: Partial<WalterPersonEntry>;
</script>

<WalterAdresse adresse={person?.then((e) => e.adresse)} />
<Row>
	<WalterTextInput
		bind:binding={binding.email}
		labelText="E-Mail"
		value={person?.then((e) => e.email)}
	/>
	<WalterTextInput
		bind:binding={binding.fax}
		labelText="Fax"
		value={person?.then((e) => e.fax)}
	/>
</Row>
<Row>
	<WalterTextInput
		bind:binding={binding.telefon}
		labelText="Telefon"
		value={person?.then((e) => e.telefon)}
	/>
	<WalterTextInput
		bind:binding={binding.mobil}
		labelText="Mobil"
		value={person?.then((e) => e.mobil)}
	/>
</Row>
<Row>
	<WalterTextInput
		bind:binding={binding.notiz}
		labelText="Notiz"
		value={person?.then((e) => e.notiz)}
	/>
</Row>
