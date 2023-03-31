<script lang="ts">
	import { WalterVertraege, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList title="Verträge" />
<WalterVertraege
	wohnungen={data.wohnungen}
	kontakte={data.kontakte}
	search
	rows={data.rows}
/>
