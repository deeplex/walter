<script lang="ts">
	import { WalterUmlagen, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Umlagen" />
<WalterUmlagen search rows={data.rows} />
